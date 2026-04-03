"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Connector = void 0;
class Connector {
    /**
     * @param actions - Methods bound to a zustand store that tracks the state of the connector.
     * @param onError - An optional handler which will report errors thrown from event listeners.
     * Actions are used by the connector to report changes in connection status.
     */
    constructor(actions, onError) {
        this.actions = actions;
        this.onError = onError;
    }
    /**
     * Reset the state of the connector without otherwise interacting with the connection.
     */
    resetState() {
        this.actions.resetState();
    }
}
exports.Connector = Connector;
