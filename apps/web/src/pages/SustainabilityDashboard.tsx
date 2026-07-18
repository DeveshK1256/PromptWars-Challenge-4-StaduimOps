import { Leaf, Droplets, Zap, Recycle, Trash2 } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, SustainabilityMetric } from "../services/api";
import type { AsyncStatus } from "../types";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

function GaugeBar({
  label,
  icon,
  current,
  target,
  unit,
  tone,
}: {
  label: string;
  icon: React.ReactNode;
  current: number;
  target: number;
  unit: string;
  tone: "ok" | "warn" | "danger";
}) {
  const percent = Math.min(100, Math.round((current / target) * 100));
  return (
    <div className="gauge-bar">
      <div className="gauge-label">
        {icon}
        <strong>{label}</strong>
        <span className="muted">
          {current.toLocaleString()} / {target.toLocaleString()} {unit}
        </span>
      </div>
      <div className="gauge-track">
        <div className={`gauge-fill ${tone}`} style={{ width: `${percent}%` }} />
      </div>
      <span className={`gauge-percent ${tone}`}>{percent}%</span>
    </div>
  );
}

export function SustainabilityDashboard({ client, stadiumId }: { client: ApiClient; stadiumId: string }) {
  const [metrics, setMetrics] = useState<SustainabilityMetric | null>(null);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    if (!stadiumId) return;
    setStatus("loading");
    setError(null);
    try {
      setMetrics(await client.sustainability(stadiumId));
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }, [client, stadiumId]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Leaf aria-hidden="true" />}
        title="Sustainability dashboard"
        description="Energy, water, waste, recycling, and carbon tracking for venue operations and fan awareness."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Sustainability metrics loaded." />

      {metrics && (
        <>
          <div className="metric-grid">
            <article
              className={`metric ${metrics.carbonScore >= 80 ? "ok" : metrics.carbonScore >= 60 ? "warn" : "danger"}`}
            >
              <span>Carbon score</span>
              <strong>{metrics.carbonScore}/100</strong>
            </article>
            <article className={`metric ${metrics.recyclingRate >= 70 ? "ok" : "warn"}`}>
              <span>Recycling rate</span>
              <strong>{metrics.recyclingRate}%</strong>
            </article>
            <article className="metric">
              <span>Venue</span>
              <strong>{metrics.stadiumName}</strong>
            </article>
            <article className="metric">
              <span>Report date</span>
              <strong>{metrics.metricDate}</strong>
            </article>
          </div>

          <article className="panel">
            <h2>Resource consumption</h2>
            <div className="gauge-stack">
              <GaugeBar
                label="Energy"
                icon={<Zap aria-hidden="true" style={{ width: 16, height: 16 }} />}
                current={metrics.energyKwh}
                target={metrics.energyTargetKwh}
                unit="kWh"
                tone={metrics.energyKwh / metrics.energyTargetKwh < 0.75 ? "ok" : "warn"}
              />
              <GaugeBar
                label="Water"
                icon={<Droplets aria-hidden="true" style={{ width: 16, height: 16 }} />}
                current={metrics.waterLiters}
                target={metrics.waterTargetLiters}
                unit="L"
                tone={metrics.waterLiters / metrics.waterTargetLiters < 0.75 ? "ok" : "warn"}
              />
              <GaugeBar
                label="Waste"
                icon={<Trash2 aria-hidden="true" style={{ width: 16, height: 16 }} />}
                current={metrics.wasteKg}
                target={metrics.wasteTargetKg}
                unit="kg"
                tone={metrics.wasteKg / metrics.wasteTargetKg < 0.8 ? "ok" : "warn"}
              />
              <GaugeBar
                label="Recycling"
                icon={<Recycle aria-hidden="true" style={{ width: 16, height: 16 }} />}
                current={metrics.recyclingRate}
                target={100}
                unit="%"
                tone={metrics.recyclingRate >= 70 ? "ok" : metrics.recyclingRate >= 50 ? "warn" : "danger"}
              />
            </div>
          </article>

          <article className="panel">
            <h2>Eco-tips for fans</h2>
            <div className="list-stack">
              {metrics.ecoTips.map((tip, index) => (
                <div className="row-card" key={index}>
                  <div>
                    <strong>
                      <Leaf aria-hidden="true" style={{ width: 14, height: 14, display: "inline", marginRight: 6 }} />
                      Tip {index + 1}
                    </strong>
                    <span>{tip}</span>
                  </div>
                </div>
              ))}
            </div>
          </article>
        </>
      )}
    </section>
  );
}
