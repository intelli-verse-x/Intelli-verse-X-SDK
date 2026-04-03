"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.createWeb3ReactStoreAndActions = exports.MAX_SAFE_CHAIN_ID = void 0;
const address_1 = require("@ethersproject/address");
const zustand_1 = require("zustand");
/**
 * MAX_SAFE_CHAIN_ID is the upper bound limit on what will be accepted for `chainId`
 * `MAX_SAFE_CHAIN_ID = floor( ( 2**53 - 39 ) / 2 ) = 4503599627370476`
 *
 * @see {@link https://github.com/MetaMask/metamask-extension/blob/b6673731e2367e119a5fee9a454dd40bd4968948/shared/constants/network.js#L31}
 */
exports.MAX_SAFE_CHAIN_ID = 4503599627370476;
function validateChainId(chainId) {
    if (!Number.isInteger(chainId) || chainId <= 0 || chainId > exports.MAX_SAFE_CHAIN_ID) {
        throw new Error(`Invalid chainId ${chainId}`);
    }
}
function validateAccount(account) {
    return (0, address_1.getAddress)(account);
}
const DEFAULT_STATE = {
    chainId: undefined,
    accounts: undefined,
    activating: false,
};
function createWeb3ReactStoreAndActions() {
    const store = (0, zustand_1.createStore)()(() => DEFAULT_STATE);
    // flag for tracking updates so we don't clobber data when cancelling activation
    let nullifier = 0;
    /**
     * Sets activating to true, indicating that an update is in progress.
     *
     * @returns cancelActivation - A function that cancels the activation by setting activating to false,
     * as long as there haven't been any intervening updates.
     */
    function startActivation() {
        const nullifierCached = ++nullifier;
        store.setState(Object.assign(Object.assign({}, DEFAULT_STATE), { activating: true }));
        // return a function that cancels the activation iff nothing else has happened
        return () => {
            if (nullifier === nullifierCached)
                store.setState({ activating: false });
        };
    }
    /**
     * Used to report a `stateUpdate` which is merged with existing state. The first `stateUpdate` that results in chainId
     * and accounts being set will also set activating to false, indicating a successful connection.
     *
     * @param stateUpdate - The state update to report.
     */
    function update(stateUpdate) {
        // validate chainId statically, independent of existing state
        if (stateUpdate.chainId !== undefined) {
            validateChainId(stateUpdate.chainId);
        }
        // validate accounts statically, independent of existing state
        if (stateUpdate.accounts !== undefined) {
            for (let i = 0; i < stateUpdate.accounts.length; i++) {
                stateUpdate.accounts[i] = validateAccount(stateUpdate.accounts[i]);
            }
        }
        nullifier++;
        store.setState((existingState) => {
            var _a, _b;
            // determine the next chainId and accounts
            const chainId = (_a = stateUpdate.chainId) !== null && _a !== void 0 ? _a : existingState.chainId;
            const accounts = (_b = stateUpdate.accounts) !== null && _b !== void 0 ? _b : existingState.accounts;
            // ensure that the activating flag is cleared when appropriate
            let activating = existingState.activating;
            if (activating && chainId && accounts) {
                activating = false;
            }
            return { chainId, accounts, activating };
        });
    }
    /**
     * Resets connector state back to the default state.
     */
    function resetState() {
        nullifier++;
        store.setState(DEFAULT_STATE);
    }
    return [store, { startActivation, update, resetState }];
}
exports.createWeb3ReactStoreAndActions = createWeb3ReactStoreAndActions;
