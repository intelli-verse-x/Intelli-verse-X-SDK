import type { Actions, Web3ReactStore } from '@web3-react/types';
/**
 * MAX_SAFE_CHAIN_ID is the upper bound limit on what will be accepted for `chainId`
 * `MAX_SAFE_CHAIN_ID = floor( ( 2**53 - 39 ) / 2 ) = 4503599627370476`
 *
 * @see {@link https://github.com/MetaMask/metamask-extension/blob/b6673731e2367e119a5fee9a454dd40bd4968948/shared/constants/network.js#L31}
 */
export declare const MAX_SAFE_CHAIN_ID = 4503599627370476;
export declare function createWeb3ReactStoreAndActions(): [Web3ReactStore, Actions];
