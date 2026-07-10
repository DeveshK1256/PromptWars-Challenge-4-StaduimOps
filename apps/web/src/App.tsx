import {
  AlertTriangle,
  Bell,
  Bot,
  Bus,
  CheckCircle2,
  Eye,
  LogOut,
  MapPinned,
  Menu,
  Moon,
  RefreshCw,
  ShieldAlert,
  Sparkles,
  Ticket,
  UserRound,
  Waves
} from "lucide-react";
import { FormEvent, ReactNode, useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  AiAgent,
  AiChatResponse,
  AiKnowledgeDocument,
  ApiClient,
  ApiError,
  CrowdZone,
  Incident,
  MatchSummary,
  NavigationRoute,
  NotificationItem,
  OperationsOverview,
  PointOfInterest,
  Stadium
} from "./services/api";
import { clearSession, loadSession, replaceSession, saveSession, StoredSession } from "./services/session";
import { isGoogleMapsConfigured, loadGoogleMaps } from "./services/googleMaps";

type ViewKey = "fan" | "operations" | "ai" | "incidents" | "notifications";

const roleOptions = [
  "RegisteredFan",
  "Volunteer"
];

const navItems: Array<{ key: ViewKey; label: string; icon: ReactNode }> = [
  { key: "fan", label: "Fan console", icon: <Ticket aria-hidden="true" /> },
  { key: "operations", label: "Command center", icon: <Waves aria-hidden="true" /> },
  { key: "ai", label: "AI assistant", icon: <Bot aria-hidden="true" /> },
  { key: "incidents", label: "Incidents", icon: <ShieldAlert aria-hidden="true" /> },
  { key: "notifications", label: "Notifications", icon: <Bell aria-hidden="true" /> }
];

export function App() {
  const [session, setSession] = useState<StoredSession | null>(() => loadSession());
  const [activeView, setActiveView] = useState<ViewKey>("fan");
  const [menuOpen, setMenuOpen] = useState(false);
  const client = useMemo(() => new ApiClient(() => session?.accessToken ?? null), [session?.accessToken]);

  useEffect(() => {
    document.documentElement.dataset.theme = session?.theme ?? "standard";
  }, [session?.theme]);

  const setAuthenticated = useCallback(
    (auth: Parameters<typeof saveSession>[0]) => {
      setSession(saveSession(auth, session));
    },
    [session]
  );

  const updateSession = useCallback((next: StoredSession) => {
    replaceSession(next);
    setSession(next);
  }, []);

  const logout = async () => {
    const refreshToken = session?.refreshToken;
    clearSession();
    setSession(null);
    if (refreshToken) {
      try {
        await client.logout(refreshToken);
      } catch {
        // Local sign-out must still complete if the API is unavailable.
      }
    }
  };

  if (!session) {
    return <AuthScreen onAuthenticated={setAuthenticated} />;
  }

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">
        Skip to dashboard
      </a>
      <aside className={menuOpen ? "sidebar sidebar-open" : "sidebar"} aria-label="Primary navigation">
        <div className="brand-lockup">
          <div className="brand-mark" aria-hidden="true">
            SO
          </div>
          <div>
            <strong>Stadium Ops</strong>
            <span>World Cup 2026</span>
          </div>
        </div>
        <nav className="nav-stack">
          {navItems.map((item) => (
            <button
              key={item.key}
              className={activeView === item.key ? "nav-item active" : "nav-item"}
              type="button"
              onClick={() => {
                setActiveView(item.key);
                setMenuOpen(false);
              }}
              aria-current={activeView === item.key ? "page" : undefined}
            >
              {item.icon}
              <span>{item.label}</span>
            </button>
          ))}
        </nav>
        <div className="sidebar-footer">
          <button
            className="utility-button"
            type="button"
            onClick={() =>
              updateSession({
                ...session,
                theme: session.theme === "contrast" ? "standard" : "contrast"
              })
            }
          >
            <Moon aria-hidden="true" />
            {session.theme === "contrast" ? "Standard contrast" : "High contrast"}
          </button>
          <button className="utility-button danger" type="button" onClick={logout}>
            <LogOut aria-hidden="true" />
            Sign out
          </button>
        </div>
      </aside>

      <main className="workspace" id="main-content">
        <header className="topbar">
          <button
            className="icon-button mobile-only"
            type="button"
            onClick={() => setMenuOpen((value) => !value)}
            aria-label="Toggle navigation"
            title="Toggle navigation"
          >
            <Menu aria-hidden="true" />
          </button>
          <div>
            <p className="eyeless-label">Signed in as</p>
            <h1>{session.user.name}</h1>
          </div>
          <div className="profile-strip">
            <UserRound aria-hidden="true" />
            <div>
              <strong>{session.user.roles.join(", ")}</strong>
              <span>{session.user.email}</span>
            </div>
          </div>
        </header>

        {activeView === "fan" && <FanConsole client={client} />}
        {activeView === "operations" && <OperationsConsole client={client} />}
        {activeView === "ai" && <AiAssistant client={client} />}
        {activeView === "incidents" && <IncidentConsole client={client} />}
        {activeView === "notifications" && <NotificationConsole client={client} />}
      </main>
    </div>
  );
}

function AuthScreen({ onAuthenticated }: { onAuthenticated: (auth: Parameters<typeof saveSession>[0]) => void }) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);
  const client = useMemo(() => new ApiClient(() => null), []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setStatus("loading");
    setError(null);

    try {
      const email = String(data.get("email") ?? "");
      const password = String(data.get("password") ?? "");
      const auth =
        mode === "login"
          ? await client.login({ email, password, deviceName: "web-browser" })
          : await client.register({
              name: String(data.get("name") ?? ""),
              email,
              password,
              preferredLanguage: String(data.get("preferredLanguage") ?? "en"),
              accessibilityPreference: String(data.get("accessibilityPreference") ?? ""),
              requestedRole: String(data.get("requestedRole") ?? "RegisteredFan")
            });
      setStatus("success");
      onAuthenticated(auth);
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  return (
    <main className="auth-layout">
      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="brand-lockup standalone">
          <div className="brand-mark" aria-hidden="true">
            SO
          </div>
          <div>
            <strong>Stadium Ops</strong>
            <span>Enterprise scaffold</span>
          </div>
        </div>
        <h1 id="auth-title">{mode === "login" ? "Sign in to operations" : "Create a tournament account"}</h1>
        <p className="muted">
          Connects to the ASP.NET Core API. No backend success is simulated.
        </p>

        <div className="segmented" role="tablist" aria-label="Authentication mode">
          <button
            type="button"
            role="tab"
            aria-selected={mode === "login"}
            className={mode === "login" ? "selected" : ""}
            onClick={() => setMode("login")}
          >
            Sign in
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={mode === "register"}
            className={mode === "register" ? "selected" : ""}
            onClick={() => setMode("register")}
          >
            Register
          </button>
        </div>

        <form className="form-grid" onSubmit={submit}>
          {mode === "register" && (
            <>
              <label>
                Name
                <input name="name" autoComplete="name" required minLength={2} />
              </label>
              <label>
                Role
                <select name="requestedRole" defaultValue="RegisteredFan">
                  {roleOptions.map((role) => (
                    <option key={role} value={role}>
                      {role}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Preferred language
                <input name="preferredLanguage" defaultValue="en" required />
              </label>
              <label>
                Accessibility preference
                <input name="accessibilityPreference" placeholder="Optional" />
              </label>
            </>
          )}
          <label>
            Email
            <input name="email" type="email" autoComplete="email" required />
          </label>
          <label>
            Password
            <input
              name="password"
              type="password"
              autoComplete={mode === "login" ? "current-password" : "new-password"}
              required
              minLength={10}
            />
          </label>
          <button className="primary-action" type="submit" disabled={status === "loading"}>
            {status === "loading" ? <RefreshCw aria-hidden="true" className="spin" /> : <CheckCircle2 aria-hidden="true" />}
            {mode === "login" ? "Sign in" : "Create account"}
          </button>
        </form>
        <StatusNotice status={status} error={error} success="Authentication completed." />
      </section>
    </main>
  );
}

function FanConsole({ client }: { client: ApiClient }) {
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
          accessibilityRequired: data.get("accessibilityRequired") === "on"
        })
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
                <span className={poi.isAccessible ? "status-pill ok" : "status-pill"}>{poi.estimatedWaitMinutes} min</span>
              </div>
            ))}
          </div>
        </div>
        <MapPanel stadium={selectedStadium} />
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
    </section>
  );
}

function OperationsConsole({ client }: { client: ApiClient }) {
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
          <Metric label="Open incidents" value={overview.openIncidentCount} tone={overview.openIncidentCount > 0 ? "warn" : "ok"} />
          <Metric label="Critical incidents" value={overview.criticalIncidentCount} tone={overview.criticalIncidentCount > 0 ? "danger" : "ok"} />
          <Metric label="Congested zones" value={overview.congestedZoneCount} tone={overview.congestedZoneCount > 0 ? "warn" : "ok"} />
          <Metric label="Sustainability score" value={overview.averageSustainabilityScore.toFixed(1)} />
        </div>
      )}

      <article className="panel split-panel">
        <div>
          <h2>Crowd zones</h2>
          <div className="list-stack">
            {zones.length === 0 && <p className="muted">No crowd zones returned by the API.</p>}
            {zones.map((zone) => (
              <div className="row-card" key={zone.id}>
                <div>
                  <strong>{zone.name}</strong>
                  <span>{zone.stadiumName}</span>
                </div>
                <span className={zone.status === "Congested" ? "status-pill danger" : "status-pill warn"}>
                  {zone.currentDensity}/{zone.maximumCapacity} {zone.status}
                </span>
              </div>
            ))}
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

function AiAssistant({ client }: { client: ApiClient }) {
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);
  const [messages, setMessages] = useState<Array<{ role: "user" | "assistant"; text: string; meta?: string }>>([]);
  const [agents, setAgents] = useState<AiAgent[]>([]);
  const [knowledge, setKnowledge] = useState<AiKnowledgeDocument[]>([]);

  const loadMetadata = useCallback(async () => {
    try {
      const [agentData, knowledgeData] = await Promise.all([client.aiAgents(), client.aiKnowledge()]);
      setAgents(agentData);
      setKnowledge(knowledgeData);
    } catch (caught) {
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void loadMetadata();
  }, [loadMetadata]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    const prompt = String(data.get("prompt") ?? "").trim();
    const context = String(data.get("context") ?? "").trim();
    if (!prompt) {
      setError({ title: "Validation failed", detail: "Prompt is required.", status: 400 });
      setStatus("error");
      return;
    }

    setMessages((existing) => [...existing, { role: "user", text: prompt }]);
    setStatus("loading");
    setError(null);
    try {
      const response: AiChatResponse = await client.aiChat({ prompt, context });
      setMessages((existing) => [
        ...existing,
        {
          role: "assistant",
          text: response.response,
          meta: `${response.agentName} - ${response.intent} - ${(response.confidenceScore * 100).toFixed(0)}% confidence - ${response.tokensUsed} tokens${response.escalationRecommended ? " - escalation advised" : ""}`
        }
      ]);
      form.reset();
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Sparkles aria-hidden="true" />}
        title="AI assistant"
        description="Calls the Vertex AI Gemini adapter. Missing cloud configuration is reported as a service error."
      />
      <StatusNotice status={status} error={error} success="AI response received." />
      <article className="panel chat-panel">
        <div className="message-list" aria-live="polite">
          {messages.length === 0 && (
            <p className="muted">Ask for navigation, accessibility, transport, incident, or stadium operations guidance.</p>
          )}
          {messages.map((message, index) => (
            <div className={`message ${message.role}`} key={`${message.role}-${index}`}>
              <strong>{message.role === "user" ? "You" : "Assistant"}</strong>
              <p>{message.text}</p>
              {message.meta && <span>{message.meta}</span>}
            </div>
          ))}
        </div>
        <form className="form-grid" onSubmit={submit}>
          <label>
            Prompt
            <textarea name="prompt" rows={4} required placeholder="Find the least crowded accessible route to my seat." />
          </label>
          <label>
            Context
            <textarea name="context" rows={3} placeholder="Optional stadium, gate, seat, language, or role context." />
          </label>
          <button className="primary-action" type="submit" disabled={status === "loading"}>
            <Bot aria-hidden="true" />
            Send to AI service
          </button>
        </form>
      </article>
      <article className="panel split-panel">
        <div>
          <h2>Agent catalog</h2>
          <div className="list-stack">
            {agents.slice(0, 5).map((agent) => (
              <div className="row-card" key={agent.key}>
                <div>
                  <strong>{agent.displayName}</strong>
                  <span>{agent.intents.slice(0, 3).join(", ")}</span>
                </div>
                {agent.safetyCritical && <span className="status-pill warn">Safety</span>}
              </div>
            ))}
          </div>
        </div>
        <div>
          <h2>Knowledge sources</h2>
          <div className="list-stack">
            {knowledge.slice(0, 4).map((document) => (
              <div className="row-card" key={document.id}>
                <div>
                  <strong>{document.title}</strong>
                  <span>{document.category} - {document.sourceType}</span>
                </div>
                <span className="status-pill ok">{document.language}</span>
              </div>
            ))}
          </div>
        </div>
      </article>
    </section>
  );
}

function IncidentConsole({ client }: { client: ApiClient }) {
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
        description: String(data.get("description") ?? "")
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
            {incidents.length === 0 && <p className="muted">No incident list returned, or your role cannot access triage.</p>}
            {incidents.map((incident) => (
              <div className="row-card incident" key={incident.id}>
                <div>
                  <strong>{incident.category}</strong>
                  <span>
                    {incident.location} - {incident.assignedTeam ?? "Unassigned"} - {new Date(incident.createdAt).toLocaleString()}
                  </span>
                </div>
                <span className={incident.priority === "Critical" ? "status-pill danger" : "status-pill warn"}>
                  {incident.priority} {incident.status}
                </span>
                {incident.status !== "Resolved" && (
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

function NotificationConsole({ client }: { client: ApiClient }) {
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      setNotifications(await client.notifications());
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
      await client.broadcast({
        title: String(data.get("title") ?? ""),
        message: String(data.get("message") ?? ""),
        type: String(data.get("type") ?? "Operations"),
        priority: String(data.get("priority") ?? "Normal"),
        deviceToken: String(data.get("deviceToken") ?? "") || undefined
      });
      form.reset();
      await load();
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Bell aria-hidden="true" />}
        title="Notifications"
        description="In-app notifications always persist; Firebase delivery only runs when service-account configuration is present."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Notification workflow completed." />
      <article className="panel split-panel">
        <form className="form-grid" onSubmit={submit}>
          <h2>Broadcast alert</h2>
          <label>
            Title
            <input name="title" required />
          </label>
          <label>
            Message
            <textarea name="message" rows={4} required />
          </label>
          <label>
            Type
            <select name="type">
              <option value="Operations">Operations</option>
              <option value="Emergency">Emergency</option>
              <option value="Transport">Transport</option>
              <option value="Accessibility">Accessibility</option>
            </select>
          </label>
          <label>
            Priority
            <select name="priority">
              <option value="Normal">Normal</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </label>
          <label>
            Firebase installation id
            <input name="deviceToken" placeholder="Optional live FCM target" />
          </label>
          <button className="primary-action" type="submit" disabled={status === "loading"}>
            <Bell aria-hidden="true" />
            Send broadcast
          </button>
        </form>
        <div>
          <h2>Notification feed</h2>
          <div className="list-stack">
            {notifications.length === 0 && <p className="muted">No notifications returned by the API.</p>}
            {notifications.map((item) => (
              <div className="row-card" key={item.id}>
                <div>
                  <strong>{item.title}</strong>
                  <span>{item.message}</span>
                  <span>{item.externalDeliveryStatus}</span>
                </div>
                <span className={item.priority === "Critical" ? "status-pill danger" : "status-pill"}>{item.priority}</span>
              </div>
            ))}
          </div>
        </div>
      </article>
    </section>
  );
}

function MapPanel({ stadium }: { stadium: Stadium | null }) {
  const mapRef = useRef<HTMLDivElement | null>(null);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [message, setMessage] = useState("");
  const configured = isGoogleMapsConfigured();

  useEffect(() => {
    if (!stadium || !configured || !mapRef.current) {
      return;
    }

    setStatus("loading");
    loadGoogleMaps()
      .then(() => {
        if (!mapRef.current || !window.google?.maps?.Map) {
          throw new Error("Google Maps JavaScript API loaded without map constructor.");
        }

        const center = { lat: Number(stadium.latitude), lng: Number(stadium.longitude) };
        const map = new window.google.maps.Map(mapRef.current, {
          center,
          zoom: 15,
          mapTypeControl: false,
          streetViewControl: false,
          fullscreenControl: true
        });
        if (window.google.maps.Marker) {
          new window.google.maps.Marker({ position: center, map, title: stadium.name });
        }
        setStatus("success");
        setMessage("Google Maps JavaScript API loaded.");
      })
      .catch((error: Error) => {
        setStatus("error");
        setMessage(error.message);
      });
  }, [configured, stadium]);

  if (!stadium) {
    return (
      <div className="map-panel empty">
        <Eye aria-hidden="true" />
        <p>Select a stadium to load map context.</p>
      </div>
    );
  }

  return (
    <div className="map-panel">
      <div className="map-header">
        <div>
          <strong>{stadium.name}</strong>
          <span>
            {stadium.city}, {stadium.country}
          </span>
        </div>
        <a
          className="text-button"
          href={`https://www.google.com/maps/search/?api=1&query=${stadium.latitude},${stadium.longitude}`}
          target="_blank"
          rel="noreferrer"
        >
          Open Maps
        </a>
      </div>
      {configured ? (
        <>
          <div className="map-canvas" ref={mapRef} aria-label={`Google map for ${stadium.name}`} />
          <p className="notice-line" aria-live="polite">
            {status === "loading" ? "Loading Google Maps JavaScript API..." : message || "Map ready."}
          </p>
        </>
      ) : (
        <div className="map-missing" role="status">
          <AlertTriangle aria-hidden="true" />
          <p>Google Maps key is not configured. Set VITE_GOOGLE_MAPS_API_KEY to enable the live map.</p>
        </div>
      )}
    </div>
  );
}

function SectionHeader({
  icon,
  title,
  description,
  action
}: {
  icon: ReactNode;
  title: string;
  description: string;
  action?: ReactNode;
}) {
  return (
    <div className="section-header">
      <div className="section-icon">{icon}</div>
      <div>
        <h2>{title}</h2>
        <p>{description}</p>
      </div>
      {action && <div className="section-action">{action}</div>}
    </div>
  );
}

function Metric({ label, value, tone }: { label: string; value: string | number; tone?: "ok" | "warn" | "danger" }) {
  return (
    <article className={`metric ${tone ?? ""}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

function RetryButton({ onRetry, loading }: { onRetry: () => void; loading: boolean }) {
  return (
    <button className="secondary-action compact" type="button" onClick={onRetry} disabled={loading}>
      <RefreshCw aria-hidden="true" className={loading ? "spin" : ""} />
      Refresh
    </button>
  );
}

type AsyncStatus = "idle" | "loading" | "success" | "error";

function StatusNotice({ status, error, success }: { status: AsyncStatus; error: ApiError | null; success: string }) {
  if (status === "idle") {
    return null;
  }

  if (status === "loading") {
    return (
      <div className="status-line loading" role="status">
        <RefreshCw aria-hidden="true" className="spin" />
        Loading...
      </div>
    );
  }

  if (status === "success") {
    return (
      <div className="status-line success" role="status">
        <CheckCircle2 aria-hidden="true" />
        {success}
      </div>
    );
  }

  return (
    <div className="status-line error" role="alert">
      <AlertTriangle aria-hidden="true" />
      <div>
        <strong>{error?.title ?? "Request failed"}</strong>
        <span>{error?.detail ?? "The workflow did not complete."}</span>
        {error?.correlationId && <span>Correlation: {error.correlationId}</span>}
      </div>
    </div>
  );
}
