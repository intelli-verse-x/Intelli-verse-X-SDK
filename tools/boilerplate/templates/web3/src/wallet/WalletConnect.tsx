import type { ReactNode } from 'react';
import { initializeConnector, Web3ReactProvider } from '@web3-react/core';
import type { Web3ReactHooks } from '@web3-react/core';
import { MetaMask } from '@web3-react/metamask';
import { BrowserProvider } from 'ethers';
import { IVX_CONFIG } from '../config';

export const [metaMask, metaMaskHooks] = initializeConnector<MetaMask>(
  (actions) => new MetaMask({ actions })
);

const connectors: [MetaMask, Web3ReactHooks][] = [[metaMask, metaMaskHooks]];

export function WalletProvider({ children }: { children: ReactNode }) {
  return <Web3ReactProvider connectors={connectors}>{children}</Web3ReactProvider>;
}

/** MetaMask via @web3-react; WalletConnect button stubs until WC v2 connector is added. */
export function WalletConnect() {
  const accounts = metaMaskHooks.useAccounts();
  const isActive = metaMaskHooks.useIsActive();
  const provider = metaMaskHooks.useProvider();

  async function connectMetaMask() {
    try {
      await metaMask.activate(IVX_CONFIG.chainId);
    } catch (e: any) {
      window.alert(e?.message || 'MetaMask connection failed');
    }
  }

  async function connectWalletConnect() {
    void BrowserProvider;
    window.alert(
      'Add @web3-react/walletconnect-v2 and a WalletConnect Cloud projectId, then wire activate() here.'
    );
  }

  const address = accounts?.[0];

  return (
    <div className="ivx-wallet-connect">
      <p className="ivx-wallet-status">
        {isActive && address
          ? `Connected: ${address.slice(0, 6)}…${address.slice(-4)} (chain ${IVX_CONFIG.chainId})`
          : 'Wallet disconnected'}
      </p>
      <button type="button" className="ivx-btn ivx-btn-primary" onClick={() => void connectMetaMask()}>
        MetaMask
      </button>
      <button type="button" className="ivx-btn ivx-btn-secondary" onClick={() => void connectWalletConnect()}>
        WalletConnect
      </button>
      {isActive && provider && (
        <button type="button" className="ivx-btn ivx-btn-ghost" onClick={() => {
          if (metaMask.deactivate) {
            void metaMask.deactivate();
          } else {
            void metaMask.resetState();
          }
        }}>
          Disconnect
        </button>
      )}
    </div>
  );
}
