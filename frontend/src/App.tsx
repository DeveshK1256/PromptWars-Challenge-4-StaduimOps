import { useState } from "react";
import DashboardPage from "./pages/DashboardPage";
import AiChatPage from "./pages/AiChatPage";
import IncidentsPage from "./pages/IncidentsPage";
import NotificationsPage from "./pages/NotificationsPage";
import LoginPage from "./pages/LoginPage";

type Page = "dashboard" | "ai-chat" | "incidents" | "notifications" | "login";

const NAV_ITEMS: { id: Page; label: string; icon: string }[] = [
  { id: "dashboard", label: "Dashboard", icon: "🏟️" },
  { id: "ai-chat", label: "AI Assistant", icon: "🤖" },
  { id: "incidents", label: "Incidents", icon: "⚠️" },
  { id: "notifications", label: "Notifications", icon: "🔔" },
];

export default function App() {
  const [page, setPage] = useState<Page>("dashboard");
  const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem("access_token"));

  if (!isLoggedIn) {
    return <LoginPage />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-blue-950 text-white">
      {/* Top navigation */}
      <nav className="sticky top-0 z-40 bg-slate-950/80 backdrop-blur border-b border-white/10" aria-label="Main navigation">
        <div className="max-w-7xl mx-auto px-4 flex items-center justify-between h-16">
          <div className="flex items-center gap-3">
            <span className="text-2xl" aria-hidden="true">🏟️</span>
            <div>
              <span className="font-bold text-white">StadiumOps</span>
              <span className="text-slate-400 text-xs ml-2">FIFA World Cup 2026</span>
            </div>
          </div>
          <div className="flex items-center gap-1" role="menubar">
            {NAV_ITEMS.map(item => (
              <button
                key={item.id}
                role="menuitem"
                onClick={() => setPage(item.id)}
                aria-current={page === item.id ? "page" : undefined}
                className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-medium transition ${
                  page === item.id
                    ? "bg-blue-600/30 text-blue-300 border border-blue-500/40"
                    : "text-slate-400 hover:text-white hover:bg-white/5"
                }`}
              >
                <span aria-hidden="true">{item.icon}</span>
                <span className="hidden sm:inline">{item.label}</span>
              </button>
            ))}
            <button
              onClick={() => { localStorage.removeItem("access_token"); setIsLoggedIn(false); }}
              className="ml-2 text-xs text-slate-500 hover:text-red-400 transition px-3 py-2"
              aria-label="Sign out"
            >
              Sign out
            </button>
          </div>
        </div>
      </nav>

      {/* Page content */}
      {page === "dashboard" && <DashboardPage />}
      {page === "ai-chat" && <AiChatPage />}
      {page === "incidents" && <IncidentsPage />}
      {page === "notifications" && <NotificationsPage />}
    </div>
  );
}
