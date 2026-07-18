import { useEffect, useState } from "react";
import { api } from "../hooks/useApi";

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getNotifications()
      .then((d: any) => setNotifications(d.items ?? d ?? []))
      .catch(() => setNotifications([]))
      .finally(() => setLoading(false));
  }, []);

  const markRead = async (id: string) => {
    try {
      await api.markNotificationRead(id);
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    } catch {}
  };

  const demoNotifications = [
    { id: "1", type: "Emergency", title: "Medical Alert", message: "Medical team dispatched to Section 112.", priority: "Critical", isRead: false, createdAt: new Date().toISOString() },
    { id: "2", type: "Operations", title: "Gate B Delay", message: "Entry delays at Gate B due to increased crowd flow. Please use Gate C.", priority: "High", isRead: false, createdAt: new Date(Date.now() - 600000).toISOString() },
    { id: "3", type: "Info", title: "Match Kickoff", message: "The match begins in 30 minutes. Please proceed to your seats.", priority: "Normal", isRead: true, createdAt: new Date(Date.now() - 1800000).toISOString() },
  ];

  const items = notifications.length > 0 ? notifications : demoNotifications;

  const PRIORITY_ICONS: Record<string, string> = { Critical: "🔴", High: "🟠", Normal: "🔵", Low: "⚪" };

  return (
    <main id="main-content" className="p-6 max-w-3xl mx-auto">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-white">Notifications</h1>
        <p className="text-slate-400 text-sm mt-1">{items.filter(n => !n.isRead).length} unread</p>
      </div>
      {loading ? (
        <div className="space-y-3">{[1,2,3].map(i => <div key={i} className="bg-white/5 rounded-2xl h-20 animate-pulse" />)}</div>
      ) : (
        <div className="space-y-3" role="list" aria-label="Notifications">
          {items.map(n => (
            <div key={n.id} role="listitem" className={`bg-white/5 border rounded-2xl p-5 transition ${n.isRead ? "border-white/5 opacity-70" : "border-blue-500/30"}`}>
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span aria-hidden="true">{PRIORITY_ICONS[n.priority] ?? "🔵"}</span>
                    <span className="font-semibold text-white text-sm">{n.title}</span>
                    <span className="text-xs text-slate-500">{n.type}</span>
                  </div>
                  <p className="text-slate-300 text-sm">{n.message}</p>
                  <p className="text-slate-600 text-xs mt-1">{new Date(n.createdAt).toLocaleString()}</p>
                </div>
                {!n.isRead && (
                  <button onClick={() => markRead(n.id)} aria-label={`Mark notification "${n.title}" as read`}
                    className="text-xs text-blue-400 hover:text-blue-300 whitespace-nowrap border border-blue-500/30 rounded-lg px-3 py-1.5 transition">
                    Mark read
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </main>
  );
}
