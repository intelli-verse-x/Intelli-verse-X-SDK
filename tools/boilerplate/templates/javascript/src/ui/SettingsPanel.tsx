import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { IVXManager } from '../sdk-stub';
import { trackEvent } from '../analytics/satori-wiring';

const VOL_KEY = 'ivx_master_volume';
const NOTIFY_KEY = 'ivx_notify_enabled';

export function SettingsPanel() {
  const nav = useNavigate();
  const [vol, setVol] = useState(0.8);
  const [notify, setNotify] = useState(true);
  useEffect(() => {
    setVol(Number(localStorage.getItem(VOL_KEY) ?? 0.8));
    setNotify(localStorage.getItem(NOTIFY_KEY) !== '0');
  }, []);
  return (
    <div className="ivx-panel">
      <h2>Settings</h2>
      <label className="ivx-setting">
        Master volume
        <input type="range" min={0} max={1} step={0.05} value={vol} onChange={(e) => {
          const v = Number(e.target.value);
          setVol(v);
          localStorage.setItem(VOL_KEY, String(v));
        }} />
      </label>
      <label className="ivx-setting ivx-toggle">
        <input type="checkbox" checked={notify} onChange={(e) => {
          setNotify(e.target.checked);
          localStorage.setItem(NOTIFY_KEY, e.target.checked ? '1' : '0');
        }} />
        Notifications
      </label>
      <button type="button" className="ivx-btn ivx-btn-ghost" onClick={() => {
        void trackEvent('session_end', { game_id: '{{game_id}}', reason: 'logout' });
        IVXManager.getInstance().clearSession();
        nav('/', { replace: true });
      }}>Log out</button>
    </div>
  );
}
