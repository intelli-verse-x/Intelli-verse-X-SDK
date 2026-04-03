import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { IVXManager } from '../sdk-stub';
import { trackEvent } from '../analytics/satori-wiring';

/** IVXAuth surface: {@link IVXManager} guest / email / OAuth. */
const IVXAuth = IVXManager.getInstance();

export function LoginScreen() {
  const nav = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  if (IVXAuth.hasValidSession) {
    nav('/menu', { replace: true });
    return null;
  }
  async function done(method: string) {
    await trackEvent('auth_complete', { game_id: '{{game_id}}', method });
    nav('/menu', { replace: true });
  }
  return (
    <main className="ivx-panel ivx-login">
      <h2>Sign in</h2>
      {err && <p className="ivx-toast ivx-toast-error">{err}</p>}
      <button type="button" className="ivx-btn ivx-btn-primary" disabled={busy} onClick={() => {
        setBusy(true);
        IVXAuth.authenticateDevice().then(() => done('guest')).catch((e: Error) => setErr(e.message)).finally(() => setBusy(false));
      }}>Guest</button>
      <form className="ivx-form" onSubmit={(e: FormEvent) => {
        e.preventDefault();
        setBusy(true);
        IVXAuth.authenticateEmail(email.trim(), password, false).then(() => done('email')).catch((ex: Error) => setErr(ex.message)).finally(() => setBusy(false));
      }}>
        <input className="ivx-input" type="email" placeholder="Email" value={email} onChange={(c) => setEmail(c.target.value)} required />
        <input className="ivx-input" type="password" placeholder="Password" value={password} onChange={(c) => setPassword(c.target.value)} required />
        <button type="submit" className="ivx-btn ivx-btn-secondary" disabled={busy}>Email</button>
      </form>
      <button type="button" className="ivx-btn ivx-btn-ghost" disabled={busy} onClick={() => {
        const t = window.prompt('Google ID token') ?? '';
        if (!t) return;
        setBusy(true);
        IVXAuth.authenticateGoogle(t).then(() => done('google')).catch((ex: Error) => setErr(ex.message)).finally(() => setBusy(false));
      }}>Social (Google)</button>
      <button type="button" className="ivx-btn ivx-btn-ghost" disabled={busy} onClick={() => {
        const t = window.prompt('Apple ID token') ?? '';
        if (!t) return;
        setBusy(true);
        IVXAuth.authenticateApple(t).then(() => done('apple')).catch((ex: Error) => setErr(ex.message)).finally(() => setBusy(false));
      }}>Social (Apple)</button>
    </main>
  );
}
