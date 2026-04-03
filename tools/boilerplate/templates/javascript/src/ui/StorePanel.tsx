import { useCallback, useEffect, useState } from 'react';
import { IVXManager } from '../sdk-stub';
import { IVX_CONFIG } from '../config';
import { trackEvent } from '../analytics/satori-wiring';

type StoreItem = { itemId: string; name: string; sectionId: string; cost?: Record<string, number> };

const IVXStore = {
  purchase: (sectionId: string, itemId: string) =>
    IVXManager.getInstance().callRpc('hiro_store_purchase', JSON.stringify({ sectionId, itemId, gameId: IVX_CONFIG.gameId })),
};

export function StorePanel() {
  const [items, setItems] = useState<StoreItem[]>([]);
  const [toast, setToast] = useState<string | null>(null);
  const load = useCallback(async () => {
    try {
      const raw = await IVXManager.getInstance().callRpc('hiro_store_list', JSON.stringify({ gameId: IVX_CONFIG.gameId }));
      const sections = (raw.sections as { sectionId?: string; items?: StoreItem[] }[]) ?? [];
      const flat: StoreItem[] = [];
      for (const s of sections) {
        const sid = s.sectionId ?? 'default';
        for (const it of s.items ?? []) flat.push({ ...it, sectionId: sid });
      }
      setItems(flat);
    } catch {
      setItems([]);
    }
  }, []);
  useEffect(() => { void load(); }, [load]);
  return (
    <div className="ivx-panel">
      <h2>Store</h2>
      {toast && <p className="ivx-toast">{toast}</p>}
      <div className="ivx-grid">
        {items.map((it) => (
          <div key={`${it.sectionId}-${it.itemId}`} className="ivx-card">
            <h3>{it.name || it.itemId}</h3>
            <button type="button" className="ivx-btn ivx-btn-primary" onClick={() => {
              IVXStore.purchase(it.sectionId, it.itemId).then(() => {
                setToast('Purchase OK');
                void trackEvent('purchase', { game_id: '{{game_id}}', item_id: it.itemId });
              }).catch(() => setToast('Purchase failed'));
            }}>Buy</button>
          </div>
        ))}
      </div>
    </div>
  );
}
