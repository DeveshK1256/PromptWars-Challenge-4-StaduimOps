import { CalendarPlus, Map, MapPinned, Ticket } from "lucide-react";
import { FormEvent, useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, MatchSummary, NavigationRoute, PointOfInterest, Stadium } from "../services/api";
import type { AsyncStatus } from "../types";
import { MapPanel } from "../components/MapPanel";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

export function FanConsole({ client }: { client: ApiClient }) {
  const [match, setMatch] = useState<MatchSummary | null>(null);
  const [stadiums, setStadiums] = useState<Stadium[]>([]);
  const [pois, setPois] = useState<PointOfInterest[]>([]);
  const [selectedStadiumId, setSelectedStadiumId] = useState("");
  const [route, setRoute] = useState<NavigationRoute | null>(null);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      const [matchData, stadiumData] = await Promise.all([client.todayMatch(), client.stadiums()]);
      setMatch(matchData);
      setStadiums(stadiumData.items);
      const stadiumId = matchData.stadium.id;
      setSelectedStadiumId(stadiumId);
      setPois(await client.pointsOfInterest(stadiumId));
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void load();
  }, [load]);

  async function changeStadium(stadiumId: string) {
    setSelectedStadiumId(stadiumId);
    setStatus("loading");
    setError(null);
    try {
      setPois(await client.pointsOfInterest(stadiumId));
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  async function submitRoute(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setStatus("loading");
    setError(null);
    setRoute(null);
    try {
      setRoute(
        await client.route({
          stadiumId: selectedStadiumId,
          fromLocation: String(data.get("fromLocation") ?? ""),
          toLocation: String(data.get("toLocation") ?? ""),
          accessibilityRequired: data.get("accessibilityRequired") === "on",
        }),
      );
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  const selectedStadium = stadiums.find((stadium) => stadium.id === selectedStadiumId) ?? match?.stadium ?? null;

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Ticket aria-hidden="true" />}
        title="Fan console"
        description="Match information, points of interest, accessibility, route guidance, and map readiness."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Fan data loaded from API." />

      {match && (
        <article className="panel emphasis">
          <p className="eyeless-label">Next match</p>
          <h2>
            {match.homeTeam} vs {match.awayTeam}
          </h2>
          <dl className="metric-row">
            <div>
              <dt>Stage</dt>
              <dd>{match.stage}</dd>
            </div>
            <div>
              <dt>Kickoff</dt>
              <dd>{new Date(match.startsAt).toLocaleString()}</dd>
            </div>
            <div>
              <dt>Venue</dt>
              <dd>{match.stadium.name}</dd>
            </div>
          </dl>
          <div className="google-actions">
            <a
              className="secondary-action compact"
              href={buildGoogleCalendarUrl(match)}
              target="_blank"
              rel="noopener noreferrer"
            >
              <CalendarPlus aria-hidden="true" style={{ width: 16, height: 16 }} />
              Add to Google Calendar
            </a>
            <a
              className="secondary-action compact"
              href={`https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(match.stadium.name + ", Miami, FL")}&travelmode=transit`}
              target="_blank"
              rel="noopener noreferrer"
            >
              <Map aria-hidden="true" style={{ width: 16, height: 16 }} />
              Google Maps Directions
            </a>
          </div>
        </article>
      )}

      <article className="panel split-panel">
        <div>
          <h2>Stadium services</h2>
          <label>
            Stadium
            <select value={selectedStadiumId} onChange={(event) => void changeStadium(event.target.value)}>
              {stadiums.map((stadium) => (
                <option key={stadium.id} value={stadium.id}>
                  {stadium.name}
                </option>
              ))}
            </select>
          </label>
          <div className="list-stack">
            {pois.length === 0 && <p className="muted">No points of interest returned by the API.</p>}
            {pois.map((poi) => (
              <div className="row-card" key={poi.id}>
                <div>
                  <strong>{poi.name}</strong>
                  <span>
                    {poi.category} - Level {poi.level} - {poi.zone}
                  </span>
                </div>
                <span className={poi.isAccessible ? "status-pill ok" : "status-pill"}>
                  {poi.estimatedWaitMinutes} min
                </span>
              </div>
            ))}
          </div>
        </div>
        <MapPanel stadium={selectedStadium} pois={pois} route={route} />
      </article>

      <article className="panel">
        <h2>Route request</h2>
        <form className="route-grid" onSubmit={submitRoute}>
          <label>
            From
            <input name="fromLocation" defaultValue="Metro Shuttle Drop" required />
          </label>
          <label>
            To
            <input name="toLocation" defaultValue="Accessible Gate C" required />
          </label>
          <label className="checkbox-row">
            <input name="accessibilityRequired" type="checkbox" defaultChecked />
            Require accessible route
          </label>
          <button className="secondary-action" type="submit" disabled={!selectedStadiumId || status === "loading"}>
            <MapPinned aria-hidden="true" />
            Request route
          </button>
        </form>
        {route && (
          <div className="route-result" aria-live="polite">
            <strong>
              {route.fromLocation} to {route.toLocation}
            </strong>
            <span>
              {route.distanceMeters} m - {route.estimatedMinutes} min - crowd load {route.crowdLoadPercent}%
            </span>
            <p>{route.safetyNote}</p>
            <p className="notice-line">{route.recommendationNotice}</p>
          </div>
        )}
      </article>

      {selectedStadium && (
        <article className="panel">
          <h2>
            <Map aria-hidden="true" style={{ width: 18, height: 18, display: "inline", marginRight: 6 }} />
            Google Maps — {selectedStadium.name}
          </h2>
          <div className="google-maps-embed">
            <iframe
              title={`Google Maps — ${selectedStadium.name}`}
              src={`https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3575.!2d-80.2388!3d25.958!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x88d9ad1e5684303f%3A0x31e3e7c1e9d6f2e2!2sHard%20Rock%20Stadium!5e0!3m2!1sen!2sus!4v1`}
              width="100%"
              height="350"
              style={{ border: 0, borderRadius: "var(--radius)" }}
              allowFullScreen
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
            />
          </div>
          <p className="muted" style={{ marginTop: 8 }}>
            Powered by Google Maps Platform — tap for full navigation with live traffic, transit schedules, and walking
            directions.
          </p>
        </article>
      )}
    </section>
  );
}

function buildGoogleCalendarUrl(match: MatchSummary): string {
  const start = new Date(match.startsAt);
  const end = new Date(start.getTime() + 3 * 60 * 60 * 1000); // 3 hours
  const fmt = (d: Date) => d.toISOString().replace(/[-:]/g, "").split(".")[0] + "Z";
  const params = new URLSearchParams({
    action: "TEMPLATE",
    text: `FIFA WC 2026: ${match.homeTeam} vs ${match.awayTeam}`,
    dates: `${fmt(start)}/${fmt(end)}`,
    details: `${match.stage} — FIFA World Cup 2026\n\nVenue: ${match.stadium.name}\nCapacity: ${match.stadium.capacity.toLocaleString()}\n\nPowered by Stadium Ops`,
    location: match.stadium.name,
    ctz: "America/New_York",
  });
  return `https://calendar.google.com/calendar/render?${params.toString()}`;
}
