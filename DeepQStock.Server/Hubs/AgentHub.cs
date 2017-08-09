﻿using DeepQStock.Agents;
using DeepQStock.Storage;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using DeepQStock.Stocks;
using DeepQStock.Utils;
using Hangfire;
using Newtonsoft.Json;
using System.Linq;
using DeepQStock.Enums;
using System.Collections.Concurrent;

namespace DeepQStock.Server.Hubs
{
    public class AgentHub : Hub
    {
        public static ConcurrentDictionary<long, string> ActiveAgents = new ConcurrentDictionary<long, string>();

        #region << Private Properties >>       

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public RedisManager RedisManager { get; set; }
               
        /// <summary>
        /// Gets or sets the agent listeners.
        /// </summary>        
        public IDictionary<string, IList<string>> AgentListeners { get; set; }

        /// <summary>
        /// The group name template
        /// </summary>
        private static string GroupNameTemplate = "ListenersForAgent-{0}";

        #endregion

        #region << Constructor >> 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AgentHub(RedisManager ctx)
        {
            RedisManager = ctx;
            RedisManager.Subscribe(RedisPubSubChannels.OnDayComplete, (c, a) =>
            {
                OnDayComplete(JsonConvert.DeserializeObject<OnDayComplete>(a));
            });

            RedisManager.Subscribe(RedisPubSubChannels.OnSimulationComplete, (c, a) =>
            {
                OnDayComplete(JsonConvert.DeserializeObject<OnDayComplete>(a));
            });

            RedisManager.Subscribe(RedisPubSubChannels.OnTrainingEpochComplete, (c, a) =>
            {
                OnTrainingEpochComplete(JsonConvert.DeserializeObject<OnTrainingEpochCompleteArgs>(a));
            });

            AgentListeners = new Dictionary<string, IList<string>>();
        }

        #endregion

        /// <summary>
        /// Get all instance of agents
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DeepRLAgentParameters> GetAll()
        {
            var agents = new List<DeepRLAgentParameters>();
            using (var ctx = new DeepQStockContext())
            {
                agents = ctx.Agents.ToList();
            }

            return agents;
        }

        /// <summary>
        /// Get all instance of agents
        /// </summary>
        /// <returns></returns>
        public DeepRLAgentParameters GetById(long id)
        {
            DeepRLAgentParameters agent = null;
            using (var ctx = new DeepQStockContext())
            {
                agent = ctx.Agents.SingleOrDefault(a => a.Id == id);
            }

            return agent;
        }

        /// <summary>
        /// Gets the decisions.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IEnumerable<OnDayComplete> GetDecisions(long id)
        {
            var decisions = new List<OnDayComplete>();
            using (var ctx = new DeepQStockContext())
            {
                decisions = ctx.OnDaysCompleted.Where(d => d.AgentId == id).OrderBy(d => d.Date).ToList();
            }

            return decisions;
        }

        /// <summary>
        /// Called when [day complete].
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void OnDayComplete(OnDayComplete args)
        {
            var group = Clients.Group(string.Format(GroupNameTemplate, args.AgentId));
            group?.onDayComplete(args);
        }

        /// <summary>
        /// Called when [simulation complete].
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void OnSimulationComplete(OnSimulationComplete args)
        {
            var group = Clients.Group(string.Format(GroupNameTemplate, args.AgentId));
            group?.onSimulationCompleted(args);
        }

        /// <summary>
        /// On traingin epoch complete
        /// </summary>
        /// <param name="args"></param>
        public void OnTrainingEpochComplete(OnTrainingEpochCompleteArgs args)
        {
            var group = Clients.Group(string.Format(GroupNameTemplate, args.AgentId));
            group?.onTrainingEpochCompleted(args);
        }

        /// <summary>
        /// Subscribes the specified agent identifier.
        /// </summary>
        /// <param name="agentId">The agent identifier.</param>
        public void Subscribe(long id)
        {
            var groupName = string.Format(GroupNameTemplate, id);

            if (!AgentListeners.ContainsKey(groupName))
            {
                AgentListeners[groupName] = new List<string>();
            }

            var previousSubcription = AgentListeners.Where((KeyValuePair<string, IList<string>> i) => i.Value.Contains(base.Context.ConnectionId));
            if (previousSubcription.Count() > 0)
            {
                var previousGroupName = previousSubcription.First().Key;
                AgentListeners[previousGroupName].Remove(base.Context.ConnectionId);
                Groups.Remove(base.Context.ConnectionId, previousGroupName);
            }

            var listerners = AgentListeners[groupName];
            if (!listerners.Contains(base.Context.ConnectionId))
            {
                listerners.Add(base.Context.ConnectionId);
                Groups.Add(base.Context.ConnectionId, groupName);
            }

        }

        /// <summary>
        /// Save an agent
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public long Save(DeepRLAgentParameters agent)
        {
            using (var ctx = new DeepQStockContext())
            {
                ctx.Agents.Add(agent);
                ctx.SaveChanges();
            }

            Clients.All.onCreatedAgent(agent);
            return agent.Id;
        }

        /// <summary>
        /// Start the simulation of the agent agentId
        /// </summary>
        /// <param name="id"></param>
        public string Start(long id)
        {
            var jobId = BackgroundJob.Enqueue<StockExchange>(s => s.Run(JobCancellationToken.Null, id));

            ActiveAgents.TryAdd(id, jobId);
            Subscribe(id);

            return jobId;
        }

        /// <summary>
        /// Start the simulation of the agent agentId
        /// </summary>
        /// <param name="id"></param>
        public void Pause(long id)
        {
            using (var ctx = new DeepQStockContext())
            {
                var agent = ctx.Agents.Single(a => a.Id == id);
                agent.Status = AgentStatus.Paused;                

                string jobId = null;
                ActiveAgents.TryRemove(id, out jobId);

                if (!string.IsNullOrEmpty(jobId))
                {
                    BackgroundJob.Delete(jobId);
                }

                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Start the simulation of the agent agentId
        /// </summary>
        /// <param name="id"></param>
        public void Stop(long id)
        {
            using (var ctx = new DeepQStockContext())
            {
                string jobId = null;
                ActiveAgents.TryRemove(id, out jobId);
                var agent = ctx.Agents.Single(a => a.Id == id);

                agent.Status = AgentStatus.Stoped;

                if (!string.IsNullOrEmpty(jobId))
                {
                    BackgroundJob.Delete(jobId);
                }

                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Start the simulation of the agent agentId
        /// </summary>
        /// <param name="id"></param>
        public void Reset(long id)
        {

        }

        /// <summary>
        /// Removes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Remove(long id)
        {
            using (var ctx = new DeepQStockContext())
            {
                string jobId = null;
                ActiveAgents.TryRemove(id, out jobId);
                var agent = ctx.Agents.Single(a => a.Id == id);

                // Here we have two posible situations, one is if the agent is running, in this case we cannot remove the agent immediately, 
                // we need mark the agent as removed and stop the agent's job. The remove process will be handle in the shutdown process.
                // And the other situation is when the agent is not running, in that case, we can remove immediately.
                if (agent.Status == AgentStatus.Running)
                {
                    agent.Status = AgentStatus.Removed;                    

                    if (!string.IsNullOrEmpty(jobId))
                    {
                        BackgroundJob.Delete(jobId);
                    }
                }
                else
                {
                    ctx.Agents.Remove(agent);                   
                }

                ctx.SaveChanges();
            }
        }

    }
}
