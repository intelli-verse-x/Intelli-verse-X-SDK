import { useCallback, useEffect, useState } from 'react';
import { IVXManager } from '../sdk-stub';
import { IVX_CONFIG } from '../config';
import { trackEvent } from '../analytics/satori-wiring';

type Ach = { id: string; name: string; description?: string; currentCount: number; maxCount: number; completed: boolean; claimed: boolean };

export function AchievementsPanel() {
  const [list, setList] = useState<Ach[]>([]);
  const ivx = IVXManager.getInstance();
  const load = useCallback(async () => {
    try {
      const raw = await ivx.callRpc('hiro_achievements_list', JSON.stringify({ gameId: IVX_CONFIG.gameId }));
      setList((raw.achievements as Ach[]) ?? []);
    } catch {
      setList([]);
    }
  }, [ivx]);
  useEffect(() => { void load(); }, [load]);
  return (
    <div className="ivx-panel">
      <h2>Achievements</h2>
      <div className="ivx-grid">
        {list.map((a) => {
          const pct = a.maxCount ? Math.min(100, Math.round((100 * a.currentCount) / a.maxCount)) : 0;
          return (
            <div key={a.id} className="ivx-card">
              <h3>{a.name}</h3>
              <p>{a.description}</p>
              <div className="ivx-progress"><div className="ivx-progress-bar" style={{ width: `${pct}%` }} /></div>
              <button type="button" className="ivx-btn ivx-btn-secondary" disabled={a.claimed || !a.completed} onClick={() => {
                ivx.callRpc('hiro_achievements_claim', JSON.stringify({ achievementId: a.id, gameId: IVX_CONFIG.gameId }))
                  .then(() => { void load(); void trackEvent('achievement', { game_id: '{{game_id}}', achievement_id: a.id }); })
                  .catch(() => { /* noop */ });
              }}>{a.claimed ? 'Claimed' : 'Claim'}</button>
            </div>
          );
        })}
      </div>
    </div>
  );
}
