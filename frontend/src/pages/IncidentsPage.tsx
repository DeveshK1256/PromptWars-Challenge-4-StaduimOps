import { useEffect, useState } from "react";
import { api } from "../hooks/useApi";

const CATEGORIES = ["Medical", "Security", "Fire", "Lost child", "Crowd issue", "Broken facility", "Other"];
const SEVERITIES = ["Low", "Normal", "High", "Critical"];

const PRIORITY_COLORS: Record<string, string> = {
  Critical: "bg-red-500/20 text-red-300 border-red-500/30",
  High: "bg-orange-500/20 text-orange-300 border-orange-500/30",
  Normal: "bg-blue-500/20 text-blue-300 border-blue-500/30",
  Low: "bg-slate-500/20 text-slate-300 border-slate-500/30",
};

export default function IncidentsPage() {
  const [incidents, setIncidents] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ category: "Medical", severity: "Normal", location: "", description: "" });
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const loadIncidents = async () => {
    setLoading(true);
    try {
      const data = await api.getIncidents();
      setIncidents(data.items ?? []);
    } catch {
      setIncidents([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadIncidents(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.location.trim() || !form.description.trim()) {
      setFormError("Location and description are required.");
      return;
    }
    setSubmitting(true);
    setFormError(null);
    try {
      await api.createIncident(form);
      setShowForm(false);
      setForm({ category: "Medical", severity: "Normal", location: "", description: "" });
      loadIncidents();
    } catch (e: any) {
      setFormError("Failed to submit. Are you signed in with a valid token?");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main id="main-content" className="p-6 max-w-5xl mx-auto">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-white">Incident Management</h1>
          <p className="text-slate-400 text-sm mt-1">Report and track operational incidents</p>
        </div>
        <button
          onClick={() => setShowForm(!showForm)}
          aria-expanded={showForm}
          className="bg-red-600 hover:bg-red-500 text-white rounded-xl px-5 py-2.5 font-medium transition flex items-center gap-2"
        >
          <span aria-hidden="true">⚠️</span> Report Incident
        </button>
      </div>

      {showForm && (
        <div className="bg-white/5 border border-white/10 rounded-2xl p-6 mb-6" role="dialog" aria-label="Report an incident">
          <h2 className="text-lg font-semibold text-white mb-4">New Incident Report</h2>
          {formError && <div role="alert" className="text-red-300 text-sm mb-4 bg-red-900/20 border border-red-500/30 rounded-lg p-3">{formError}</div>}
          <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="incident-category" className="block text-sm font-medium text-blue-200 mb-1">Category</label>
              <select id="incident-category" value={form.category} onChange={e => setForm(f => ({ ...f, category: e.target.value }))}
                className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-white focus:outline-none focus:ring-2 focus:ring-blue-500">
                {CATEGORIES.map(c => <option key={c} value={c} className="bg-slate-800">{c}</option>)}
              </select>
            </div>
            <div>
              <label htmlFor="incident-severity" className="block text-sm font-medium text-blue-200 mb-1">Severity</label>
              <select id="incident-severity" value={form.severity} onChange={e => setForm(f => ({ ...f, severity: e.target.value }))}
                className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-white focus:outline-none focus:ring-2 focus:ring-blue-500">
                {SEVERITIES.map(s => <option key={s} value={s} className="bg-slate-800">{s}</option>)}
              </select>
            </div>
            <div>
              <label htmlFor="incident-location" className="block text-sm font-medium text-blue-200 mb-1">Location</label>
              <input id="incident-location" type="text" value={form.location} onChange={e => setForm(f => ({ ...f, location: e.target.value }))}
                placeholder="e.g. Section 112, Gate B" required
                className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div className="md:col-span-2">
              <label htmlFor="incident-description" className="block text-sm font-medium text-blue-200 mb-1">Description</label>
              <textarea id="incident-description" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                placeholder="Describe the incident..." required rows={3}
                className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none" />
            </div>
            <div className="md:col-span-2 flex gap-3">
              <button type="submit" disabled={submitting}
                className="bg-red-600 hover:bg-red-500 disabled:opacity-50 text-white rounded-xl px-6 py-2.5 font-medium transition">
                {submitting ? "Submitting…" : "Submit Report"}
              </button>
              <button type="button" onClick={() => setShowForm(false)}
                className="bg-white/10 hover:bg-white/20 text-white rounded-xl px-6 py-2.5 font-medium transition">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="space-y-3">{[1,2,3].map(i => <div key={i} className="bg-white/5 rounded-2xl h-20 animate-pulse" />)}</div>
      ) : incidents.length === 0 ? (
        <div className="text-center py-16 text-slate-500">
          <div className="text-4xl mb-3" aria-hidden="true">✅</div>
          <p className="text-lg">No incidents reported</p>
          <p className="text-sm mt-1">All clear! The stadium is operating normally.</p>
        </div>
      ) : (
        <div className="space-y-3" role="list" aria-label="Incident reports">
          {incidents.map((inc: any) => (
            <article key={inc.id} role="listitem" className="bg-white/5 border border-white/10 rounded-2xl p-5 hover:bg-white/8 transition">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span className={`px-2 py-0.5 rounded-full border text-xs font-medium ${PRIORITY_COLORS[inc.priority] ?? PRIORITY_COLORS.Normal}`}>{inc.priority}</span>
                    <span className="text-white font-medium">{inc.category}</span>
                    <span className="text-slate-400 text-sm">• {inc.severity}</span>
                  </div>
                  <div className="text-slate-300 text-sm">📍 {inc.location}</div>
                  {inc.assignedTeam && <div className="text-slate-400 text-xs mt-1">Assigned to: {inc.assignedTeam}</div>}
                </div>
                <div className="text-right">
                  <div className="text-xs text-slate-500">{inc.status}</div>
                  <div className="text-xs text-slate-600 mt-0.5">{new Date(inc.createdAt).toLocaleString()}</div>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </main>
  );
}
