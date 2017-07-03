
import { Injectable, EventEmitter } from '@angular/core';
import { Agent } from '../models/agent';
import { OnDayComplete } from '../models/on-day-complete';
import { BaseService } from './base-service';

/**
 * Agent Service
 * 
 * @export
 * @class AgentService
 */
@Injectable()
export class AgentService extends BaseService {

    // public events    
    public onDayCompleted: EventEmitter<OnDayComplete>;
    public onCreatedAgent: EventEmitter<Agent>;

    /**
     * Creates an instance of AgentService.
     * 
     * @memberof AgentService
     */
    constructor() {
        super('agentHub');

        this.onDayCompleted = new EventEmitter<OnDayComplete>();
        this.onCreatedAgent = new EventEmitter<Agent>();

        this.proxy.on('onCreatedAgent', a => this.onCreatedAgent.emit(a));
        this.proxy.on('onDayComplete', a => this.onDayCompleted.emit(a));

        this.init();
    }

    /**
     * Get all agents
     */
    public getAll(): Promise<Agent[]> {
        return this.execute('getAll');
    }

    /**
     * Get an agent by id 
     * @param id
     */
    public getById(id: number): Promise<Agent> {
        return this.execute('getById', id);
    }

    /**
     * Save an agent
     * @param agent 
     */
    public save(agent: Agent) {
        return this.execute('save', agent);
    }

    public getDecisions(id: number) {
        return this.execute('getDecisions', id);
    }

    /**
     * Start the simulation of a particular agent      
     * @memberof AgentService
     */
    public start(id: number): void {
        this.execute('start', id);
    }

    /**
     * Stop the simulation of a particular agent      
     * @memberof AgentService
     */
    public pause(id: number): void {
        this.execute('pause', id);
    }

    /**
     * Stop the simulation of a particular agent      
     * @memberof AgentService
     */
    public stop(id: number): void {
        this.execute('stop', id);
    }

    /**
     * Reset the learning of an agent
     * @memberof AgentService
     */
    public reset(id: number): void {
        this.execute('reset', id);
    }
}