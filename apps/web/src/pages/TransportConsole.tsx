import { Bus, Clock, RefreshCw, Train, Car, ParkingSquare } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, TransportStatus } from "../services/api";
import type { AsyncStatus } from "../types";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

function modeIcon(mode: string) {
  switch (mode.toLowerCase()) {
    case "metro":
      return <Train aria-hidden="true" />;
    case "bus":
      return <Bus aria-hidden="true" />;
    case "rideshare":
      return <Car aria-hidden="true" />;
    case "parking":
      return <ParkingSquare aria-hidden="true" />;
    default:
      return <Bus aria-hidden="true" />;
  }
}

function statusTone(status: string) {
  if (status === "On Time" || status === "Available") return "ok";
  if (status === "Delayed") return "warn";
  if (status === "High Demand") return "warn";
  return "";
}

export function TransportConsole({ client }: { client: ApiClient }) {
  const [transports, setTransports] = useState<TransportStatus[]>([]);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      setTransports(await client.transportStatus());
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
        icon={<Bus aria-hidden="true" />}
        title="Transportation intelligence"
        description="Real-time transit status, shuttle tracking, rideshare demand, and parking availability for match day."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Transport data loaded." />

      <div className="metric-grid">
        {transports.map((transport) => (
          <article className="panel transport-card" key={transport.id}>
            <div className="transport-header">
              <div className="transport-icon">{modeIcon(transport.mode)}</div>
              <div>
                <strong>{transport.mode}</strong>
                <span className="muted">{transport.provider}</span>
              </div>
              <span className={`status-pill ${statusTone(transport.status)}`}>{transport.status}</span>
            </div>
            <p className="transport-route">{transport.route}</p>
            <div className="transport-meta">
              {transport.estimatedDelayMinutes > 0 && (
                <span className="status-pill warn">
                  <Clock aria-hidden="true" style={{ width: 14, height: 14 }} />
                  {transport.estimatedDelayMinutes} min delay
                </span>
              )}
              {transport.nextArrival && (
                <span className="muted">
                  <RefreshCw aria-hidden="true" style={{ width: 12, height: 12 }} />
                  Next: {new Date(transport.nextArrival).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                </span>
              )}
            </div>
          </article>
        ))}
      </div>

      <article className="panel">
        <h2>AI transport recommendations</h2>
        <p className="muted">
          Ask the AI Assistant for personalized transport guidance — &quot;What&apos;s the fastest way to Hard Rock
          Stadium right now?&quot; or &quot;Find accessible parking near Gate C.&quot;
        </p>
        <div className="readiness-list">
          <p>
            🚇 <strong>Metro</strong> is recommended for lowest carbon footprint and fastest arrival.
          </p>
          <p>
            🚌 <strong>FIFA Fan Shuttle</strong> runs express from Downtown Miami every 15 minutes.
          </p>
          <p>
            ♿ <strong>Accessible parking</strong> available in Lot 18 — 340 spaces remaining.
          </p>
        </div>
      </article>
    </section>
  );
}
