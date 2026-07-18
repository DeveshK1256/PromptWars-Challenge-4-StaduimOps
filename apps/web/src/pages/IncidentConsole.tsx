import { AlertTriangle, ShieldAlert } from "lucide-react";
import { FormEvent, useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, Incident } from "../services/api";
import type { AsyncStatus } from "../types";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

export function IncidentConsole({ client, userRoles }: { client: ApiClient; userRoles: string[] }) {
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      setIncidents((await client.incidents()).items);
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void load();
  }, [load]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    setStatus("loading");
    setError(null);
    try {
      await client.createIncident({
        category: String(data.get("category") ?? ""),
        severity: String(data.get("severity") ?? ""),
        location: String(data.get("location") ?? ""),
        description: String(data.get("description") ?? ""),
      });
      form.reset();
      await load();
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  async function resolve(id: string) {
    setStatus("loading");
    setError(null);
    try {
      await client.updateIncidentStatus(id, "Resolved");
      await load();
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<ShieldAlert aria-hidden="true" />}
        title="Incident response"
        description="Report incidents as any authenticated user; triage list is protected by incident-response roles."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Incident workflow completed." />
      <article className="panel split-panel">
        <form className="form-grid" onSubmit={submit}>
          <h2>Report incident</h2>
          <label>
            Category
            <select name="category" required>
              <option value="Medical">Medical</option>
              <option value="Lost child">Lost child</option>
              <option value="Security">Security</option>
              <option value="Fire">Fire</option>
              <option value="Crowd issue">Crowd issue</option>
              <option value="Broken facility">Broken facility</option>
            </select>
          </label>
          <label>
            Severity
            <select name="severity" required>
              <option value="Normal">Normal</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </label>
          <label>
            Location
            <input name="location" required placeholder="Gate, section, concourse, or landmark" />
          </label>
          <label>
            Description
            <textarea name="description" rows={4} required />
          </label>
          <button className="primary-action" type="submit" disabled={status === "loading"}>
            <AlertTriangle aria-hidden="true" />
            Submit incident
          </button>
        </form>
        <div>
          <h2>Triage queue</h2>
          <div className="list-stack">
            {incidents.length === 0 && (
              <p className="muted">No incident list returned, or your role cannot access triage.</p>
            )}
            {incidents.map((incident) => (
              <div className="row-card incident" key={incident.id}>
                <div>
                  <strong>{incident.category}</strong>
                  <span>
                    {incident.location} - {incident.assignedTeam ?? "Unassigned"} -{" "}
                    {new Date(incident.createdAt).toLocaleString()}
                  </span>
                </div>
                <span className={incident.priority === "Critical" ? "status-pill danger" : "status-pill warn"}>
                  {incident.priority} {incident.status}
                </span>
                {incident.status !== "Resolved" && userRoles.includes("Volunteer") && (
                  <button className="text-button" type="button" onClick={() => void resolve(incident.id)}>
                    Mark resolved
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      </article>
    </section>
  );
}
