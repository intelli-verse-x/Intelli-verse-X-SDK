import { useCallback, useEffect, useState } from 'react';
import { IVXManager, type IVXLeaderboardRecord } from '../sdk-stub';

const LB_GLOBAL = 'global_leaderboard';
const LB_FRIENDS = 'friends_leaderboard';

export function LeaderboardPanel() {
  const [tab, setTab] = useState<'global' | 'friends'>('global');
  const [rows, setRows] = useState<IVXLeaderboardRecord[]>([]);
  const ivx = IVXManager.getInstance();
  const load = useCallback(async () => {
    const id = tab === 'global' ? LB_GLOBAL : LB_FRIENDS;
    try {
      setRows(await ivx.fetchLeaderboard(id, 30));
    } catch {
      setRows([]);
    }
  }, [ivx, tab]);
  useEffect(() => { void load(); }, [load]);
  return (
    <div className="ivx-panel">
      <h2>Leaderboard</h2>
      <div className="ivx-subtabs">
        <button type="button" className={tab === 'global' ? 'ivx-tab ivx-tab-active' : 'ivx-tab'} onClick={() => setTab('global')}>Global</button>
        <button type="button" className={tab === 'friends' ? 'ivx-tab ivx-tab-active' : 'ivx-tab'} onClick={() => setTab('friends')}>Friends</button>
      </div>
      <ol className="ivx-rank-list">
        {rows.map((r) => (
          <li key={r.ownerId} className="ivx-rank-row">
            <span className="ivx-rank-num">#{r.rank}</span>
            <span className="ivx-rank-name">{r.username || r.ownerId.slice(0, 8)}</span>
            <span className="ivx-rank-score">{r.score}</span>
          </li>
        ))}
      </ol>
    </div>
  );
}
