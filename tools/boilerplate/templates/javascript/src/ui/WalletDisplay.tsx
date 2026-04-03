import { useCallback, useEffect, useState } from 'react';
import { IVXManager } from '../sdk-stub';

/** IVXEconomy-style balances via {@link IVXManager#fetchWallet} and wallet events. */
const IVXEconomy = IVXManager.getInstance();

function pick(w: Record<string, unknown>): { coins: number; gems: number } {
  const c = w.currencies as Record<string, number> | undefined;
  if (c && typeof c === 'object') {
    return { coins: Number(c.coins ?? 0), gems: Number(c.gems ?? 0) };
  }
  return { coins: Number(w.coins ?? 0), gems: Number(w.gems ?? 0) };
}

export function WalletDisplay() {
  const [coins, setCoins] = useState(0);
  const [gems, setGems] = useState(0);
  const refresh = useCallback(async () => {
    if (!IVXEconomy.hasValidSession) return;
    try {
      const w = await IVXEconomy.fetchWallet();
      const b = pick(w as Record<string, unknown>);
      setCoins(b.coins);
      setGems(b.gems);
    } catch {
      /* keep last */
    }
  }, []);
  useEffect(() => {
    void refresh();
    const onWallet = (wallet: Record<string, number>) => {
      const b = pick(wallet as unknown as Record<string, unknown>);
      setCoins(b.coins);
      setGems(b.gems);
    };
    IVXEconomy.on('walletUpdated', onWallet);
    return () => IVXEconomy.off('walletUpdated', onWallet);
  }, [refresh]);
  return (
    <div className="ivx-wallet">
      <span className="ivx-wallet-pill">Coins: <strong>{coins}</strong></span>
      <span className="ivx-wallet-pill">Gems: <strong>{gems}</strong></span>
    </div>
  );
}
