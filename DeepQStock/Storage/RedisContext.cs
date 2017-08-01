﻿using DeepQStock.Agents;
using DeepQStock.Domain;
using DeepQStock.Indicators;
using DeepQStock.Stocks;
using StackExchange.Redis;
using System;
using System.Linq;

namespace DeepQStock.Storage
{
    public class RedisContext
    {
        //Storages
        public BaseStorage<StockExchangeParameters> StockExchanges { get; set; }
        public BaseStorage<DeepRLAgentParameters> Agents { get; set; }
        public BaseStorage<QNetworkParameters> QNetworks { get; set; }
        public BaseStorage<SimulationResult> SimulationResults { get; set; }

        public OnDayCompleteStorage OnDayCompleted { get; set; }
        public TechnicalIndicatorStorage Indicators { get; set; }
        public StateStorage StateStorage { get; set; }

        // Privates
        private ISubscriber Subscriber { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisContext"/> class.
        /// </summary>
        /// <param name="redis">The redis.</param>
        public RedisContext(IConnectionMultiplexer redis)
        {
            QNetworks = new BaseStorage<QNetworkParameters>(redis);
            Agents = new BaseStorage<DeepRLAgentParameters>(redis);
            StockExchanges = new BaseStorage<StockExchangeParameters>(redis);
            SimulationResults = new BaseStorage<SimulationResult>(redis);            

            OnDayCompleted = new OnDayCompleteStorage(redis);
            StateStorage = new StateStorage(redis);
            Indicators = new TechnicalIndicatorStorage(redis);

            Subscriber = redis.GetSubscriber();
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="message">The message.</param>
        public void Publish(string channel, string message)
        {
            Subscriber.Publish(channel, message);
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="message">The message.</param>
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            Subscriber.Subscribe(channel, handler);
        }

        /// <summary>
        /// Removes the agent.
        /// </summary>
        public void RemoveAgent(DeepRLAgentParameters agent)
        {
            var decisions = OnDayCompleted.GetAll().Where(d => d.AgentId == agent.Id);
            if (decisions.Count() > 0)
            {
                OnDayCompleted.DeleteByIds(decisions.Select(d => d.Id));
            }

            var currentState = StateStorage.GetById(agent.StockExchange.CurrentStateId);

            StateStorage.Delete(currentState);

            var indicatorsIds = Indicators.GetAll().Where(i => i.StockExchangeId == agent.StockExchangeId).Select(i => i.Id).ToList();
            Indicators.DeleteByIds(indicatorsIds);
            
            StockExchanges.Delete(agent.StockExchange);

            QNetworks.Delete(agent.QNetwork);

            //TODO: Remove the simulation results.

            Agents.Delete(agent);
        }
    }
}
