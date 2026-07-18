import { useEffect, useState } from "react";
import { api } from "../hooks/useApi";

interface MatchInfo {
  homeTeam: string;
  awayTeam: string;
  startsAt: string;
  stage: string;
  status: string;
  stadium?: { name: string; city: string; country: string; capacity: number };
}

interface CrowdZone {
  id: string;
  name: string;
  currentDensity: number;
  maximumCapacity: number;
  status: string;
}

function StatusBadge({ status }: { status: string }) {
  const colors: Record<string, string> = {
    Normal: "bg-emerald-500/20 text-emerald-300 border-emerald-500/30",
    Crowded: "bg-amber-500/20 text-amber-300 border-amber-500/30",
    Critical: "bg-red-500/20 text-red-300 border-red-500/30",
    Scheduled: "bg-blue-500/20 text-blue-300 border-blue-500/30",
    Live: "bg-green-500/20 text-green-300 border-green-500/30",
  };
  return (
    <span className={`px-2 py-0.5 rounded-full border text-xs font-medium ${colors[status] ?? "bg-slate-500/20 text-slate-300 border-slate-500/30"}`}>
      {status}
    </span>
  );
}

export default function DashboardPage() {
  const [match, setMatch] = useState<MatchInfo | null>(null);
  const [stadiums, setStadiums] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    Promise.allSettled([
      api.getTodayMatch().then(setMatch).catch(() => {}),
      api.getStadiums().then((d: any) => setStadiums(d.items ?? [])).catch(() => {}),
    ]).finally(() => setLoading(false));
  }, []);

  const demoZones: CrowdZone[] = [
    { id: "1", name: "North Gate", currentDensity: 1200, maximumCapacity: 2000, status: "Normal" },
    { id: "2", name: "South Concourse", currentDensity: 3800, maximumCapacity: 4000, status: "Crowded" },
    { id: "3", name: "VIP Entrance", currentDensity: 150, maximumCapacity: 500, status: "Normal" },
    { id: "4", name: "Food Village", currentDensity: 2900, maximumCapacity: 3000, status: "Critical" },
  ];

  return (
    <main id="main-content" className="p-6 max-w-7xl mx-auto">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-white">Operations Dashboard</h1>
        <p className="text-slate-400 mt-1">FIFA World Cup 2026 — Real-time Stadium Intelligence</p>
      </div>

      {loading && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 animate-pulse">
          {[1,2,3].map(i => <div key={i} className="bg-white/5 rounded-2xl h-32" />)}
        </div>
      )}

      {!loading && (
        <>
          {/* Match Banner */}
          {match && (
            <div className="bg-gradient-to-r from-blue-900/60 to-indigo-900/60 border border-blue-500/30 rounded-2xl p-6 mb-6 backdrop-blur">
              <div className="flex items-center justify-between flex-wrap gap-4">
                <div>
                  <div className="text-slate-400 text-sm mb-1">Today's Match • {match.stage}</div>
                  <div className="text-3xl font-bold text-white">
                    {match.homeTeam} <span className="text-blue-400">vs</span> {match.awayTeam}
                  </div>
                  <div className="text-slate-300 mt-1 text-sm">
                    🏟️ {match.stadium?.name}, {match.stadium?.city} &nbsp;•&nbsp; 🕐 {new Date(match.startsAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                  </div>
                </div>
                <StatusBadge status={match.status} />
              </div>
            </div>
          )}

          {/* Stats row */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            {[
              { label: "Stadiums", value: stadiums.length || "11", icon: "🏟️", color: "from-blue-600 to-blue-800" },
              { label: "Active Crowd Zones", value: demoZones.length, icon: "👥", color: "from-violet-600 to-violet-800" },
              { label: "Open Incidents", value: "3", icon: "⚠️", color: "from-amber-600 to-amber-800" },
              { label: "AI Queries Today", value: "1,247", icon: "🤖", color: "from-emerald-600 to-emerald-800" },
            ].map(stat => (
              <div key={stat.label} className={`bg-gradient-to-br ${stat.color} rounded-2xl p-5`}>
                <div className="text-2xl mb-2" aria-hidden="true">{stat.icon}</div>
                <div className="text-3xl font-bold text-white">{stat.value}</div>
                <div className="text-white/70 text-sm mt-1">{stat.label}</div>
              </div>
            ))}
          </div>

          {/* Crowd Zones */}
          <div className="bg-white/5 border border-white/10 rounded-2xl p-6 mb-6">
            <h2 className="text-lg font-semibold text-white mb-4">Crowd Zone Status</h2>
            <div className="space-y-3">
              {demoZones.map(zone => {
                const pct = Math.round((zone.currentDensity / zone.maximumCapacity) * 100);
                const barColor = pct >= 90 ? "bg-red-500" : pct >= 75 ? "bg-amber-500" : "bg-emerald-500";
                return (
                  <div key={zone.id} className="flex items-center gap-4">
                    <div className="w-32 text-sm text-slate-300 shrink-0">{zone.name}</div>
                    <div className="flex-1 bg-white/10 rounded-full h-3 overflow-hidden">
                      <div className={`${barColor} h-full rounded-full transition-all duration-500`} style={{ width: `${Math.min(pct, 100)}%` }} role="progressbar" aria-valuenow={pct} aria-valuemin={0} aria-valuemax={100} aria-label={`${zone.name} density at ${pct}%`} />
                    </div>
                    <div className="w-12 text-right text-sm text-slate-300">{pct}%</div>
                    <StatusBadge status={zone.status} />
                  </div>
                );
              })}
            </div>
          </div>

          {/* Stadiums list */}
          {stadiums.length > 0 && (
            <div className="bg-white/5 border border-white/10 rounded-2xl p-6">
              <h2 className="text-lg font-semibold text-white mb-4">World Cup 2026 Venues</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {stadiums.map((s: any) => (
                  <div key={s.id} className="bg-white/5 border border-white/10 rounded-xl p-4 hover:bg-white/10 transition">
                    <div className="font-medium text-white text-sm">{s.name}</div>
                    <div className="text-slate-400 text-xs mt-0.5">{s.city}, {s.country}</div>
                    <div className="text-blue-400 text-xs mt-1">Capacity: {s.capacity?.toLocaleString()}</div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </main>
  );
}
