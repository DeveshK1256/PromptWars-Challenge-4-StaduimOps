import {
  Bell,
  Bot,
  Bus,
  HandHeart,
  Leaf,
  LogOut,
  Menu,
  Moon,
  ShieldAlert,
  Ticket,
  UserRound,
  Waves,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { ApiClient } from "./services/api";
import { clearSession, loadSession, replaceSession, saveSession } from "./services/session";
import type { StoredSession } from "./services/session";
import { LiveTicker } from "./components/LiveTicker";
import { AiAssistant } from "./pages/AiAssistant";
import { AuthScreen } from "./pages/AuthScreen";
import { FanConsole } from "./pages/FanConsole";
import { IncidentConsole } from "./pages/IncidentConsole";
import { NotificationConsole } from "./pages/NotificationConsole";
import { OperationsConsole } from "./pages/OperationsConsole";
import { SustainabilityDashboard } from "./pages/SustainabilityDashboard";
import { TransportConsole } from "./pages/TransportConsole";
import { VolunteerConsole } from "./pages/VolunteerConsole";
import type { ViewKey } from "./types";

const navItems: Array<{ key: ViewKey; label: string; icon: ReactNode }> = [
  { key: "fan", label: "Fan console", icon: <Ticket aria-hidden="true" /> },
  { key: "operations", label: "Command center", icon: <Waves aria-hidden="true" /> },
  { key: "ai", label: "AI assistant", icon: <Bot aria-hidden="true" /> },
  { key: "transport", label: "Transportation", icon: <Bus aria-hidden="true" /> },
  { key: "sustainability", label: "Sustainability", icon: <Leaf aria-hidden="true" /> },
  { key: "volunteer", label: "Volunteer hub", icon: <HandHeart aria-hidden="true" /> },
  { key: "incidents", label: "Incidents", icon: <ShieldAlert aria-hidden="true" /> },
  { key: "notifications", label: "Notifications", icon: <Bell aria-hidden="true" /> },
];

const DEFAULT_STADIUM_ID = "stadium-hard-rock-miami";

export function App() {
  const [session, setSession] = useState<StoredSession | null>(() => loadSession());
  const [activeView, setActiveView] = useState<ViewKey>("fan");
  const [menuOpen, setMenuOpen] = useState(false);
  const client = useMemo(() => new ApiClient(() => session?.accessToken ?? null), [session?.accessToken]);

  useEffect(() => {
    document.documentElement.dataset.theme = session?.theme ?? "standard";
  }, [session?.theme]);

  const setAuthenticated = useCallback(
    (auth: Parameters<typeof saveSession>[0]) => {
      setSession(saveSession(auth, session));
    },
    [session],
  );

  const updateSession = useCallback((next: StoredSession) => {
    replaceSession(next);
    setSession(next);
  }, []);

  const logout = async () => {
    const refreshToken = session?.refreshToken;
    clearSession();
    setSession(null);
    if (refreshToken) {
      try {
        await client.logout(refreshToken);
      } catch {
        // Local sign-out must still complete if the API is unavailable.
      }
    }
  };

  if (!session) {
    return <AuthScreen onAuthenticated={setAuthenticated} />;
  }

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">
        Skip to dashboard
      </a>
      <aside className={menuOpen ? "sidebar sidebar-open" : "sidebar"} aria-label="Primary navigation">
        <div className="brand-lockup">
          <div className="brand-mark" aria-hidden="true">
            SO
          </div>
          <div>
            <strong>Stadium Ops</strong>
            <span>World Cup 2026</span>
          </div>
        </div>
        <nav className="nav-stack">
          {navItems.map((item) => (
            <button
              key={item.key}
              className={activeView === item.key ? "nav-item active" : "nav-item"}
              type="button"
              onClick={() => {
                setActiveView(item.key);
                setMenuOpen(false);
              }}
              aria-current={activeView === item.key ? "page" : undefined}
            >
              {item.icon}
              <span>{item.label}</span>
            </button>
          ))}
        </nav>
        <div className="sidebar-footer">
          <button
            className="utility-button"
            type="button"
            onClick={() =>
              updateSession({
                ...session,
                theme: session.theme === "contrast" ? "standard" : "contrast",
              })
            }
          >
            <Moon aria-hidden="true" />
            {session.theme === "contrast" ? "Standard contrast" : "High contrast"}
          </button>
          <button className="utility-button danger" type="button" onClick={logout}>
            <LogOut aria-hidden="true" />
            Sign out
          </button>
        </div>
      </aside>

      <main className="workspace" id="main-content">
        <LiveTicker client={client} />
        <header className="topbar">
          <button
            className="icon-button mobile-only"
            type="button"
            onClick={() => setMenuOpen((value) => !value)}
            aria-label="Toggle navigation"
            title="Toggle navigation"
          >
            <Menu aria-hidden="true" />
          </button>
          <div>
            <p className="eyeless-label">Signed in as</p>
            <h1>{session.user.name}</h1>
          </div>
          <div className="profile-strip">
            <UserRound aria-hidden="true" />
            <div>
              <strong>{session.user.roles.join(", ")}</strong>
              <span>{session.user.email}</span>
            </div>
          </div>
        </header>

        {activeView === "fan" && <FanConsole client={client} />}
        {activeView === "operations" && <OperationsConsole client={client} />}
        {activeView === "ai" && <AiAssistant client={client} />}
        {activeView === "transport" && <TransportConsole client={client} />}
        {activeView === "sustainability" && <SustainabilityDashboard client={client} stadiumId={DEFAULT_STADIUM_ID} />}
        {activeView === "volunteer" && <VolunteerConsole client={client} />}
        {activeView === "incidents" && <IncidentConsole client={client} userRoles={session.user.roles} />}
        {activeView === "notifications" && <NotificationConsole client={client} />}
      </main>
    </div>
  );
}
