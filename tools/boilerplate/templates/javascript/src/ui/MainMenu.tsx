import { useState } from 'react';
import { WalletDisplay } from './WalletDisplay';
import { StorePanel } from './StorePanel';
import { DailyRewardsPanel } from './DailyRewardsPanel';
import { LeaderboardPanel } from './LeaderboardPanel';
import { AchievementsPanel } from './AchievementsPanel';
import { SettingsPanel } from './SettingsPanel';
import { trackEvent } from '../analytics/satori-wiring';

const TABS = ['home', 'store', 'achievements', 'daily', 'leaderboard', 'settings'] as const;
type TabId = (typeof TABS)[number];

export function MainMenu() {
  const [tab, setTab] = useState<TabId>('home');
  return (
    <div className="ivx-main">
      <WalletDisplay />
      <nav className="ivx-tabs" role="tablist">
        {TABS.map((id) => (
          <button
            key={id}
            type="button"
            role="tab"
            aria-selected={tab === id}
            className={tab === id ? 'ivx-tab ivx-tab-active' : 'ivx-tab'}
            onClick={() => {
              setTab(id);
              void trackEvent('screen_view', { game_id: '{{game_id}}', screen: id });
            }}
          >
            {id === 'daily' ? 'Daily rewards' : id[0].toUpperCase() + id.slice(1)}
          </button>
        ))}
      </nav>
      <section className="ivx-tab-panel">
        {tab === 'home' && (
          <div className="ivx-panel">
            <h2>Home</h2>
            <p>{'Welcome to {{game_name}}. {{tagline}}'}</p>
          </div>
        )}
        {tab === 'store' && <StorePanel />}
        {tab === 'achievements' && <AchievementsPanel />}
        {tab === 'daily' && <DailyRewardsPanel />}
        {tab === 'leaderboard' && <LeaderboardPanel />}
        {tab === 'settings' && <SettingsPanel />}
      </section>
    </div>
  );
}
