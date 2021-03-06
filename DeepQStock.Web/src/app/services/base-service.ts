
import { environment } from '../../environments/environment';

/**
 * Base Service
 * 
 * @export
 * @class BaseService
 */
export class BaseService {

    // Private fields
    protected connection: SignalR.Hub.Connection;
    protected proxy: SignalR.Hub.Proxy;
    protected onConnected: Promise<any>;
    protected hubName:string;


    /**
     * Creates an instance of BaseService.
     * @param {string} hubName 
     * 
     * @memberof BaseService
     */
    constructor(hubName: string) {

        this.hubName = hubName;
        this.connection = $.hubConnection(environment.signalrUrl);
        this.connection.logging = environment.signalRloggingEnabled;
        this.proxy = this.connection.createHubProxy(hubName);
    }


    /**
     * Start the connection
     * 
     * @protected
     * 
     * @memberof BaseService
     */
    protected init() {
        this.onConnected = Promise.resolve(this.connection.start().then(() => console.log(`connnection established with ${this.hubName}`)));
    }

    /**
     * execute a method in the server
     * 
     * @protected
     * @param {string} method 
     * @param {...any[]} args 
     * @returns 
     * 
     * @memberof BaseService
     */
    protected execute(method: string, ...parameters: any[]) {
        return this.onConnected.then(() => {
            var args = [method].concat(parameters);

            return Promise.resolve(this.proxy.invoke.apply(this.proxy, args));
        });
    }
}