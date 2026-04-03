import { useCallback, useEffect, useState } from 'react';
import { IVXHiroSystems } from '../sdk-stub';

export function DailyRewardsPanel() {
  const [days, setDays] = useState<{ day: number; claimed: boolean }[]>([]);
  const [streak, setStreak] = useState(0);
  const [busy, setBusy] = useState(false);
  const hi = IVXHiroSystems.getInstance();
  const load = useCallback(async () => {
    try {
      const r = await hi.retention.get();
      setDays(r.rewards ?? []);
      const streaks = await hi.streaks.get();
      const login = streaks.find((s: any) => s.streakId === 'daily_login') ?? streaks[0];
      setStreak(login?.currentStreak ?? 0);
    } catch {
      setDays(Array.from({ length: 7 }, (_, i) => ({ day: i + 1, claimed: false })));
    }
  }, [hi]);
  useEffect(() => { void load(); }, [load]);
  return (
    <div className="ivx-panel">
      <h2>Daily rewards</h2>
      <p className="ivx-streak">Streak: <strong>{streak}</strong> days</p>
      <div className="ivx-calendar">
        {(days.length ? days : Array.from({ length: 7 }, (_, i) => ({ day: i + 1, claimed: false }))).map((d, i) => (
          <div key={i} className={d.claimed ? 'ivx-cal-cell ivx-cal-claimed' : 'ivx-cal-cell'}>Day {d.day ?? i + 1}</div>
        ))}
      </div>
      <button type="button" className="ivx-btn ivx-btn-primary" disabled={busy} onClick={() => {
        setBusy(true);
        hi.retention.update().then(() => load()).finally(() => setBusy(false));
      }}>Claim / check in</button>
    </div>
  );
}
