import { Radio } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { ApiClient, LiveAlert } from "../services/api";

export function LiveTicker({ client }: { client: ApiClient }) {
  const [alerts, setAlerts] = useState<LiveAlert[]>([]);

  const load = useCallback(async () => {
    try {
      setAlerts(await client.liveAlerts());
    } catch {
      // Ticker is non-critical — silently fail
    }
  }, [client]);

  useEffect(() => {
    void load();
    const interval = window.setInterval(() => void load(), 30_000);
    return () => window.clearInterval(interval);
  }, [load]);

  if (alerts.length === 0) return null;

  return (
    <div className="live-ticker" role="marquee" aria-label="Live operational alerts">
      <span className="ticker-badge">
        <Radio aria-hidden="true" style={{ width: 14, height: 14 }} />
        LIVE
      </span>
      <div className="ticker-track">
        {/* Render twice for seamless infinite scroll loop */}
        {[...alerts, ...alerts].map((alert, i) => (
          <span className={`ticker-item ${alert.severity}`} key={`${alert.id}-${i}`}>
            <strong>{alert.type}:</strong> {alert.message}
          </span>
        ))}
      </div>
    </div>
  );
}
