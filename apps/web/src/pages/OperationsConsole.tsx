import { AlertTriangle, CheckCircle2, Waves } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, CrowdZone, OperationsOverview } from "../services/api";
import type { AsyncStatus } from "../types";
import { Metric } from "../components/Metric";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

export function OperationsConsole({ client }: { client: ApiClient }) {
  const [overview, setOverview] = useState<OperationsOverview | null>(null);
  const [zones, setZones] = useState<CrowdZone[]>([]);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      const [overviewData, crowdData] = await Promise.all([client.operationsOverview(), client.crowdZones()]);
      setOverview(overviewData);
      setZones(crowdData);
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Waves aria-hidden="true" />}
        title="Command center"
        description="Role-protected operations overview, crowd status, incident load, and live-integration readiness."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Operations data loaded from API." />

      {overview && (
        <div className="metric-grid">
          <Metric label="Stadiums" value={overview.stadiumCount} />
          <Metric label="Matches" value={overview.matchCount} />
          <Metric
            label="Open incidents"
            value={overview.openIncidentCount}
            tone={overview.openIncidentCount > 0 ? "warn" : "ok"}
          />
          <Metric
            label="Critical incidents"
            value={overview.criticalIncidentCount}
            tone={overview.criticalIncidentCount > 0 ? "danger" : "ok"}
          />
          <Metric
            label="Congested zones"
            value={overview.congestedZoneCount}
            tone={overview.congestedZoneCount > 0 ? "warn" : "ok"}
          />
          <Metric label="Sustainability score" value={overview.averageSustainabilityScore.toFixed(1)} />
        </div>
      )}

      <article className="panel split-panel">
        <div>
          <h2>Crowd density heatmap</h2>
          <div className="list-stack">
            {zones.length === 0 && <p className="muted">No crowd zones returned by the API.</p>}
            {zones.map((zone) => {
              const percent = Math.round((zone.currentDensity / zone.maximumCapacity) * 100);
              const tone = percent >= 80 ? "danger" : percent >= 60 ? "warn" : "ok";
              return (
                <div className="row-card crowd-heatmap-row" key={zone.id}>
                  <div>
                    <strong>{zone.name}</strong>
                    <span>{zone.stadiumName}</span>
                  </div>
                  <div className="crowd-bar-container">
                    <div className="gauge-track">
                      <div className={`gauge-fill ${tone}`} style={{ width: `${percent}%` }} />
                    </div>
                    <span className={`gauge-percent ${tone}`}>
                      {zone.currentDensity}/{zone.maximumCapacity} ({percent}%)
                    </span>
                  </div>
                  <span className={`status-pill ${tone}`}>{zone.status}</span>
                </div>
              );
            })}
          </div>
        </div>
        <div>
          <h2>Integration readiness</h2>
          <ul className="readiness-list">
            {(overview?.aiInsightReadiness ?? []).map((item) => (
              <li key={item}>
                {item.includes("pending") ? <AlertTriangle aria-hidden="true" /> : <CheckCircle2 aria-hidden="true" />}
                {item}
              </li>
            ))}
          </ul>
        </div>
      </article>
    </section>
  );
}
