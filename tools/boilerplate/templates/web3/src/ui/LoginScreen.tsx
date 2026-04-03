import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { IVXManager } from '../sdk-stub';

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

  return (
    <main className="ivx-panel ivx-login">
      <h2>Sign in</h2>
      {err && <p className="ivx-toast ivx-toast-error">{err}</p>}
      <button
        type="button"
        className="ivx-btn ivx-btn-primary"
        disabled={busy}
        onClick={() => {
          setBusy(true);
          IVXAuth.authenticateDevice()
            .then(() => nav('/menu', { replace: true }))
            .catch((e: Error) => setErr(e.message))
            .finally(() => setBusy(false));
        }}
      >
        Guest
      </button>
      <form
        className="ivx-form"
        onSubmit={(e: FormEvent) => {
          e.preventDefault();
          setBusy(true);
          IVXAuth.authenticateEmail(email.trim(), password, false)
            .then(() => nav('/menu', { replace: true }))
            .catch((ex: Error) => setErr(ex.message))
            .finally(() => setBusy(false));
        }}
      >
        <input
          className="ivx-input"
          type="email"
          placeholder="Email"
          value={email}
          onChange={(c) => setEmail(c.target.value)}
          required
        />
        <input
          className="ivx-input"
          type="password"
          placeholder="Password"
          value={password}
          onChange={(c) => setPassword(c.target.value)}
          required
        />
        <button type="submit" className="ivx-btn ivx-btn-secondary" disabled={busy}>
          Email
        </button>
      </form>
    </main>
  );
}
