import { createElement } from 'react';
import { createRoot } from 'react-dom/client';
import { IVXManager, IVXHiroSystems } from './sdk-stub';

import { buildIvManagerConfig } from './config';
import { initSatori, identifySatoriUser, trackEvent } from './analytics/satori-wiring';
import { App } from './ui/App';
import './styles/theme.css';

const client = IVXManager.getInstance();

function wireHiroAfterAuth(): void {
  const c = client.client;
  const s = client.session;
  if (c && s) IVXHiroSystems.getInstance().initialize(c, s);
}

client.initialize(buildIvManagerConfig());
client.on('authSuccess', (userId: string) => {
  wireHiroAfterAuth();
  void identifySatoriUser(userId);
});
if (client.restoreSession()) {
  wireHiroAfterAuth();
  void identifySatoriUser(client.userId);
}

void initSatori();
void trackEvent('session_start', { game_id: '{{game_id}}', source: 'app_boot' });

const root = document.getElementById('root');
if (root) createRoot(root).render(createElement(App));
