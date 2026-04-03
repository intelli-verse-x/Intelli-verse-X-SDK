import type { ReactNode } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { IVXManager } from '../sdk-stub';

import { LoginScreen } from './LoginScreen';
import { MainMenu } from './MainMenu';
import { WalletProvider, WalletConnect } from '../wallet/WalletConnect';

function RequireAuth({ children }: { children: ReactNode }) {
  const ivx = IVXManager.getInstance();
  if (!ivx.hasValidSession) return <Navigate to="/" replace />;
  return <>{children}</>;
}

export function App() {
  return (
    <WalletProvider>
      <BrowserRouter>
        <div className="ivx-app">
          <header className="ivx-app-header">
            <h1 className="ivx-title">{'{{game_name}}'}</h1>
            <p className="ivx-tagline">{'{{tagline}}'}</p>
            <WalletConnect />
          </header>
          <Routes>
            <Route path="/" element={<LoginScreen />} />
            <Route path="/menu" element={<RequireAuth><MainMenu /></RequireAuth>} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </div>
      </BrowserRouter>
    </WalletProvider>
  );
}
