export type ApiError = {
  title?: string;
  detail?: string;
  status?: number;
  correlationId?: string;
};

export type ApiEnvelope<T> = {
  success: boolean;
  data: T;
  error?: {
    code: string;
    message: string;
  };
  correlationId: string;
};

export type PagedEnvelope<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type UserProfile = {
  id: string;
  name: string;
  email: string;
  preferredLanguage: string;
  accessibilityPreference?: string;
  roles: string[];
};

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserProfile;
};

export type Stadium = {
  id: string;
  name: string;
  city: string;
  country: string;
  capacity: number;
  latitude: number;
  longitude: number;
};

export type MatchSummary = {
  id: string;
  homeTeam: string;
  awayTeam: string;
  startsAt: string;
  stage: string;
  status: string;
  stadium: Stadium;
};

export type PointOfInterest = {
  id: string;
  name: string;
  category: string;
  level: string;
  zone: string;
  isAccessible: boolean;
  estimatedWaitMinutes: number;
};

export type NavigationRoute = {
  id: string;
  fromLocation: string;
  toLocation: string;
  distanceMeters: number;
  estimatedMinutes: number;
  isAccessible: boolean;
  crowdLoadPercent: number;
  safetyNote: string;
  recommendationNotice: string;
};

export type AiChatResponse = {
  conversationId: string;
  response: string;
  intent: string;
  model: string;
  tokensUsed: number;
  agentName: string;
  confidenceScore: number;
  escalationRecommended: boolean;
  sources: string[];
};

export type AiAgent = {
  key: string;
  displayName: string;
  description: string;
  intents: string[];
  responsibilities: string[];
  requiresOperationalRole: boolean;
  safetyCritical: boolean;
};

export type AiKnowledgeDocument = {
  id: string;
  category: string;
  title: string;
  sourceType: string;
  sourceUri?: string;
  contentSummary: string;
  language: string;
  isApproved: boolean;
};

export type Incident = {
  id: string;
  category: string;
  severity: string;
  priority: string;
  location: string;
  status: string;
  assignedTeam?: string;
  createdAt: string;
};

export type CrowdZone = {
  id: string;
  stadiumName: string;
  name: string;
  currentDensity: number;
  maximumCapacity: number;
  status: string;
  lastUpdated: string;
};

export type OperationsOverview = {
  stadiumCount: number;
  matchCount: number;
  openIncidentCount: number;
  criticalIncidentCount: number;
  congestedZoneCount: number;
  activeVolunteerTasks: number;
  averageSustainabilityScore: number;
  aiInsightReadiness: string[];
};

export type NotificationItem = {
  id: string;
  title: string;
  message: string;
  type: string;
  priority: string;
  isRead: boolean;
  externalDeliveryStatus: string;
};

export type TransportStatus = {
  id: string;
  mode: string;
  provider: string;
  route: string;
  status: string;
  estimatedDelayMinutes: number;
  nextArrival: string;
  lastUpdated: string;
};

export type SustainabilityMetric = {
  id: string;
  stadiumName: string;
  energyKwh: number;
  energyTargetKwh: number;
  waterLiters: number;
  waterTargetLiters: number;
  wasteKg: number;
  wasteTargetKg: number;
  recyclingRate: number;
  carbonScore: number;
  metricDate: string;
  ecoTips: string[];
};

export type VolunteerTask = {
  id: string;
  title: string;
  location: string;
  status: string;
  priority: string;
  category: string;
  startsAt: string;
  endsAt: string;
  description: string;
};

export type TranslationResponse = {
  originalText: string;
  translatedText: string;
  fromLanguage: string;
  toLanguage: string;
  model: string;
};

export type LiveAlert = {
  id: string;
  type: string;
  message: string;
  timestamp: string;
  severity: string;
};

const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();
const API_BASE_URL = configuredApiBaseUrl || (import.meta.env.PROD ? "demo" : "http://localhost:5000/api/v1");

export const apiMode = API_BASE_URL === "demo" ? "demo" : "live";

export class ApiClient {
  constructor(private readonly getAccessToken: () => string | null) {}

  async register(payload: {
    name: string;
    email: string;
    password: string;
    preferredLanguage: string;
    accessibilityPreference?: string;
    requestedRole: string;
  }) {
    return this.request<AuthResponse>("/auth/register", {
      method: "POST",
      body: payload,
      auth: false,
    });
  }

  async login(payload: { email: string; password: string; deviceName: string }) {
    return this.request<AuthResponse>("/auth/login", {
      method: "POST",
      body: payload,
      auth: false,
    });
  }

  async logout(refreshToken: string) {
    return this.request<{ message: string }>("/auth/logout", {
      method: "POST",
      body: { refreshToken },
    });
  }

  async me() {
    return this.request<UserProfile>("/auth/me");
  }

  async todayMatch() {
    return this.request<MatchSummary>("/matches/today");
  }

  async stadiums() {
    return this.request<PagedEnvelope<Stadium>>("/stadiums");
  }

  async pointsOfInterest(stadiumId: string) {
    return this.request<PointOfInterest[]>(`/stadiums/${stadiumId}/pois`);
  }

  async route(payload: {
    stadiumId: string;
    fromLocation: string;
    toLocation: string;
    accessibilityRequired: boolean;
  }) {
    return this.request<NavigationRoute>("/navigation/routes", {
      method: "POST",
      body: payload,
    });
  }

  async aiChat(payload: { prompt: string; context?: string }) {
    return this.request<AiChatResponse>("/ai/chat", {
      method: "POST",
      body: payload,
    });
  }

  async aiAgents() {
    return this.request<AiAgent[]>("/ai/agents");
  }

  async aiKnowledge() {
    return this.request<AiKnowledgeDocument[]>("/ai/knowledge");
  }

  async incidents() {
    return this.request<PagedEnvelope<Incident>>("/incidents");
  }

  async createIncident(payload: { category: string; severity: string; location: string; description: string }) {
    return this.request<Incident>("/incidents", {
      method: "POST",
      body: payload,
    });
  }

  async updateIncidentStatus(id: string, status: string, assignedTeam?: string) {
    return this.request<Incident>(`/incidents/${id}/status`, {
      method: "PATCH",
      body: { status, assignedTeam },
    });
  }

  async operationsOverview() {
    return this.request<OperationsOverview>("/operations/overview");
  }

  async crowdZones() {
    return this.request<CrowdZone[]>("/crowd/zones");
  }

  async notifications() {
    return this.request<NotificationItem[]>("/notifications");
  }

  async broadcast(payload: { title: string; message: string; type: string; priority: string; deviceToken?: string }) {
    return this.request<NotificationItem>("/notifications/broadcast", {
      method: "POST",
      body: payload,
    });
  }

  async transportStatus() {
    return this.request<TransportStatus[]>("/transport/status");
  }

  async sustainability(stadiumId: string) {
    return this.request<SustainabilityMetric>(`/stadiums/${stadiumId}/sustainability`);
  }

  async volunteerTasks() {
    return this.request<VolunteerTask[]>("/volunteer/tasks");
  }

  async updateVolunteerTask(id: string, status: string) {
    return this.request<VolunteerTask>(`/volunteer/tasks/${id}`, {
      method: "PATCH",
      body: { status },
    });
  }

  async translate(payload: { text: string; fromLanguage: string; toLanguage: string }) {
    return this.request<TranslationResponse>("/ai/translate", {
      method: "POST",
      body: payload,
    });
  }

  async liveAlerts() {
    return this.request<LiveAlert[]>("/alerts/live");
  }

  private async request<T>(
    path: string,
    options: {
      method?: string;
      body?: unknown;
      auth?: boolean;
    } = {},
  ): Promise<T> {
    const headers: Record<string, string> = {
      Accept: "application/json",
      "X-Correlation-ID": crypto.randomUUID(),
    };

    if (options.body !== undefined) {
      headers["Content-Type"] = "application/json";
    }

    if (options.auth !== false) {
      const token = this.getAccessToken();
      if (!token) {
        throw {
          title: "Not signed in",
          detail: "Sign in before using this workflow.",
          status: 401,
        } satisfies ApiError;
      }
      headers.Authorization = `Bearer ${token}`;
    }

    if (apiMode === "demo") {
      return demoRequest<T>(path, options.body);
    }

    let response: Response;
    try {
      response = await fetch(`${API_BASE_URL}${path}`, {
        method: options.method ?? "GET",
        headers,
        body: options.body === undefined ? undefined : JSON.stringify(options.body),
      });
    } catch {
      throw {
        title: "Network error",
        detail: `Could not reach the API at ${API_BASE_URL}. Start the ASP.NET API or update VITE_API_BASE_URL.`,
        status: 0,
      } satisfies ApiError;
    }

    const payload = await parseJson(response);
    if (!response.ok) {
      throw normalizeProblem(payload, response.status);
    }

    if (payload && typeof payload === "object" && "success" in payload) {
      const envelope = payload as ApiEnvelope<T>;
      if (!envelope.success) {
        throw {
          title: envelope.error?.code ?? "Request failed",
          detail: envelope.error?.message ?? "The API reported a failure.",
          correlationId: envelope.correlationId,
          status: response.status,
        } satisfies ApiError;
      }
      return envelope.data;
    }

    return payload as T;
  }
}

async function parseJson(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) {
    return undefined;
  }

  try {
    return JSON.parse(text);
  } catch {
    return { title: "Invalid response", detail: text, status: response.status };
  }
}

function normalizeProblem(payload: unknown, status: number): ApiError {
  if (payload && typeof payload === "object") {
    const data = payload as Record<string, unknown>;
    return {
      title: asString(data.title) ?? `HTTP ${status}`,
      detail: asString(data.detail) ?? asString(data.message) ?? "The API request failed.",
      status,
      correlationId: asString(data.correlationId),
    };
  }

  return {
    title: `HTTP ${status}`,
    detail: "The API request failed.",
    status,
  };
}

function asString(value: unknown) {
  return typeof value === "string" ? value : undefined;
}

const demoStadiumHardRock: Stadium = {
  id: "stadium-hard-rock-miami",
  name: "Hard Rock Stadium",
  city: "Miami Gardens",
  country: "United States",
  capacity: 65326,
  latitude: 25.9579,
  longitude: -80.2389,
};

const demoStadiumArrowhead: Stadium = {
  id: "stadium-arrowhead-kc",
  name: "Arrowhead Stadium",
  city: "Kansas City",
  country: "United States",
  capacity: 76416,
  latitude: 39.0489,
  longitude: -94.4839,
};

const demoPois: PointOfInterest[] = [
  {
    id: "poi-accessible-gate-c",
    name: "Accessible Gate C",
    category: "Entrance",
    level: "Ground",
    zone: "North",
    isAccessible: true,
    estimatedWaitMinutes: 6,
  },
  {
    id: "poi-global-food-hall",
    name: "Global Food Hall",
    category: "Food",
    level: "2",
    zone: "East",
    isAccessible: true,
    estimatedWaitMinutes: 12,
  },
  {
    id: "poi-medical-204",
    name: "Medical Station 204",
    category: "Medical",
    level: "2",
    zone: "West",
    isAccessible: true,
    estimatedWaitMinutes: 0,
  },
];

const demoAgents: AiAgent[] = [
  {
    key: "fan-assistant",
    displayName: "Fan Assistant Agent",
    description: "Answers stadium, match-day, food, merchandise, and lost-and-found questions.",
    intents: ["General", "FanExperience", "Food"],
    responsibilities: ["Stadium FAQs", "Match information", "Food recommendations"],
    requiresOperationalRole: false,
    safetyCritical: false,
  },
  {
    key: "smart-navigation",
    displayName: "Smart Navigation Agent",
    description: "Provides indoor, outdoor, accessible, and crowd-aware routing guidance.",
    intents: ["Navigation", "Route", "SeatFinding"],
    responsibilities: ["Accessible routing", "Crowd-aware routes", "Route recalculation"],
    requiresOperationalRole: false,
    safetyCritical: false,
  },
  {
    key: "accessibility-assistant",
    displayName: "Accessibility Assistant",
    description:
      "Prioritizes wheelchair-safe routes, accessible entrances, elevator guidance, and assistive experiences.",
    intents: ["Accessibility", "AccessibleRoute"],
    responsibilities: ["Wheelchair routing", "Accessible entrances", "Elevator guidance"],
    requiresOperationalRole: false,
    safetyCritical: false,
  },
  {
    key: "operations-intelligence",
    displayName: "Operations Intelligence Agent",
    description: "Summarizes operational conditions, incidents, resource needs, and staff allocation.",
    intents: ["Operations", "IncidentSummary", "Staffing"],
    responsibilities: ["Operational summaries", "Incident prioritization", "Staff allocation"],
    requiresOperationalRole: true,
    safetyCritical: true,
  },
  {
    key: "emergency-response",
    displayName: "Emergency Response Agent",
    description: "Supports emergency guidance and responder escalation without replacing protocols.",
    intents: ["Emergency", "Medical", "Security", "Evacuation"],
    responsibilities: ["Medical guidance", "Security escalation", "Evacuation guidance"],
    requiresOperationalRole: true,
    safetyCritical: true,
  },
];

const demoKnowledge: AiKnowledgeDocument[] = [
  {
    id: "knowledge-accessibility",
    category: "Accessibility",
    title: "Demo Accessibility Routing Guide",
    sourceType: "StaticDemo",
    contentSummary:
      "Use accessible gates, elevators, and staff checkpoints for wheelchair and mobility-support routes.",
    language: "en",
    isApproved: true,
  },
  {
    id: "knowledge-emergency",
    category: "Emergency Procedures",
    title: "Demo Emergency Escalation Guidance",
    sourceType: "StaticDemo",
    contentSummary:
      "AI guidance is advisory. Safety issues must be escalated to venue responders and official protocols.",
    language: "en",
    isApproved: true,
  },
  {
    id: "knowledge-transport",
    category: "Transportation",
    title: "Demo Transportation Assistance Guide",
    sourceType: "StaticDemo",
    contentSummary:
      "Recommend shuttles, public transit, and accessible pickup areas when configured data is available.",
    language: "en",
    isApproved: true,
  },
];

const demoCrowdZones: CrowdZone[] = [
  {
    id: "crowd-south-gates",
    stadiumName: demoStadiumHardRock.name,
    name: "South Gates Plaza",
    currentDensity: 71,
    maximumCapacity: 100,
    status: "Congested",
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "crowd-north-concourse",
    stadiumName: demoStadiumHardRock.name,
    name: "North Concourse",
    currentDensity: 58,
    maximumCapacity: 100,
    status: "Elevated",
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "crowd-east-food-hall",
    stadiumName: demoStadiumHardRock.name,
    name: "East Food Hall",
    currentDensity: 82,
    maximumCapacity: 100,
    status: "Congested",
    lastUpdated: new Date().toISOString(),
  },
];

const initialDemoIncidents: Incident[] = [
  {
    id: "incident-demo-1",
    category: "Crowd",
    severity: "Medium",
    priority: "High",
    location: "East Food Hall",
    status: "Open",
    assignedTeam: "Operations",
    createdAt: new Date().toISOString(),
  },
];

const initialDemoNotifications: NotificationItem[] = [
  {
    id: "notification-demo-1",
    title: "Welcome to Stadium Ops",
    message: "Static demo mode is active until a deployed API URL is configured.",
    type: "System",
    priority: "Normal",
    isRead: false,
    externalDeliveryStatus: "static-demo",
  },
];

async function demoRequest<T>(path: string, body: unknown): Promise<T> {
  await new Promise((resolve) => window.setTimeout(resolve, 120));

  if (path === "/auth/register") {
    const payload = body as Partial<{
      name: string;
      email: string;
      preferredLanguage: string;
      accessibilityPreference: string;
      requestedRole: string;
    }>;
    const email = payload.email || "fan@example.com";
    const name = payload.name || "Demo Fan";
    const role = payload.requestedRole || "RegisteredFan";
    // Store registered user in local registry
    const registry = readDemoRegistry();
    if (registry[email]) {
      throw {
        title: "Already registered",
        detail: `An account with email ${email} already exists. Please sign in instead.`,
        status: 409,
      } satisfies ApiError;
    }
    registry[email] = { name, role, preferredLanguage: payload.preferredLanguage || "en", accessibilityPreference: payload.accessibilityPreference };
    localStorage.setItem("stadium-ops-demo-users", JSON.stringify(registry));
    return demoAuth(
      name,
      email,
      payload.preferredLanguage,
      payload.accessibilityPreference,
      role,
    ) as T;
  }

  if (path === "/auth/login") {
    const payload = body as Partial<{ email: string }>;
    const email = payload.email || "";
    const registry = readDemoRegistry();
    const user = registry[email];
    if (!user) {
      throw {
        title: "Invalid credentials",
        detail: `No account found for ${email}. Please register first.`,
        status: 401,
      } satisfies ApiError;
    }
    return demoAuth(
      user.name,
      email,
      user.preferredLanguage,
      user.accessibilityPreference,
      user.role,
    ) as T;
  }

  if (path === "/auth/logout") {
    return { message: "Demo session cleared." } as T;
  }

  if (path === "/auth/me") {
    return demoAuth("Demo Fan", "fan@example.com", "en", undefined, "RegisteredFan").user as T;
  }

  if (path === "/matches/today") {
    // Quarter-Final 1 — Norway vs England at Hard Rock Stadium, Miami
    // Kickoff: 5:00 PM ET (21:00 UTC) on July 11, 2026
    return {
      id: "match-qf1-norway-england",
      homeTeam: "Norway",
      awayTeam: "England",
      startsAt: "2026-07-11T21:00:00Z",
      stage: "Quarter-Final",
      status: "Scheduled",
      stadium: demoStadiumHardRock,
    } as T;
  }

  if (path === "/stadiums") {
    return page([demoStadiumHardRock, demoStadiumArrowhead]) as T;
  }

  if (path.startsWith("/stadiums/") && path.endsWith("/pois")) {
    return demoPois as T;
  }

  if (path === "/navigation/routes") {
    const payload = body as Partial<{ fromLocation: string; toLocation: string; accessibilityRequired: boolean }>;
    return {
      id: demoId("route"),
      fromLocation: payload.fromLocation || "Metro Shuttle Drop",
      toLocation: payload.toLocation || "Accessible Gate C",
      distanceMeters: payload.accessibilityRequired ? 420 : 280,
      estimatedMinutes: payload.accessibilityRequired ? 7 : 5,
      isAccessible: Boolean(payload.accessibilityRequired),
      crowdLoadPercent: payload.accessibilityRequired ? 38 : 61,
      safetyNote: payload.accessibilityRequired
        ? "Accessible static demo route with curb-free access and staff checkpoints."
        : "Static demo route. Follow posted signs and venue staff instructions.",
      recommendationNotice: "Static demo recommendation. Connect the ASP.NET API for live routing.",
    } as T;
  }

  if (path === "/ai/agents") {
    return demoAgents as T;
  }

  if (path === "/ai/knowledge") {
    return demoKnowledge as T;
  }

  if (path === "/ai/chat") {
    const payload = body as Partial<{ prompt: string }>;
    const prompt = payload.prompt || "";
    const emergency = /medical|emergency|security|fire|evacuation/i.test(prompt);
    const accessibility = /wheelchair|accessible|elevator/i.test(prompt);
    return {
      conversationId: demoId("conversation"),
      response: emergency
        ? "This is advisory demo guidance. Contact venue staff or emergency responders immediately and share your exact location."
        : accessibility
          ? "Use Accessible Gate C, follow elevator signage, and ask staff at the checkpoint for step-free routing."
          : "Static demo response: connect the deployed ASP.NET API and Vertex AI configuration for live grounded assistance.",
      intent: emergency ? "Emergency" : accessibility ? "Accessibility" : "General",
      model: "static-demo",
      tokensUsed: 0,
      agentName: emergency
        ? "Emergency Response Agent"
        : accessibility
          ? "Accessibility Assistant"
          : "Fan Assistant Agent",
      confidenceScore: emergency ? 0.94 : 0.82,
      escalationRecommended: emergency,
      sources: demoKnowledge.map((document) => document.category),
    } as T;
  }

  if (path === "/ai/conversations") {
    return [] as T;
  }

  if (path === "/incidents" && body) {
    return createDemoIncident(body) as T;
  }

  if (path === "/incidents") {
    return page(readDemoList("stadium-ops-demo-incidents", initialDemoIncidents)) as T;
  }

  if (path.startsWith("/incidents/") && path.endsWith("/status")) {
    return updateDemoIncident(path, body) as T;
  }

  if (path === "/operations/overview") {
    const incidents = readDemoList("stadium-ops-demo-incidents", initialDemoIncidents);
    return {
      stadiumCount: 2,
      matchCount: 2,
      openIncidentCount: incidents.filter((incident) => incident.status !== "Resolved").length,
      criticalIncidentCount: incidents.filter((incident) => incident.priority === "Critical").length,
      congestedZoneCount: demoCrowdZones.filter((zone) => zone.status === "Congested").length,
      activeVolunteerTasks: 24,
      averageSustainabilityScore: 87,
      aiInsightReadiness: [
        "FIFA WC 2026 Quarter-Final day active",
        "2 venues operational (Miami · Kansas City)",
        "Deploy ASP.NET API for live Vertex AI integrations",
      ],
    } as T;
  }

  if (path === "/crowd/zones") {
    return demoCrowdZones as T;
  }

  if (path === "/notifications") {
    return readDemoList("stadium-ops-demo-notifications", initialDemoNotifications) as T;
  }

  if (path === "/notifications/broadcast") {
    return createDemoNotification(body) as T;
  }

  if (path === "/transport/status") {
    return demoTransportStatuses as T;
  }

  if (path.startsWith("/stadiums/") && path.endsWith("/sustainability")) {
    return demoSustainability as T;
  }

  if (path === "/volunteer/tasks" && !body) {
    return readDemoList("stadium-ops-demo-volunteer-tasks", initialDemoVolunteerTasks) as T;
  }

  if (path.startsWith("/volunteer/tasks/") && body) {
    return updateDemoVolunteerTask(path, body) as T;
  }

  if (path === "/volunteer/tasks" && body) {
    return readDemoList("stadium-ops-demo-volunteer-tasks", initialDemoVolunteerTasks) as T;
  }

  if (path === "/ai/translate") {
    return demoTranslate(body) as T;
  }

  if (path === "/alerts/live") {
    return demoLiveAlerts() as T;
  }

  throw {
    title: "Static demo route missing",
    detail: `The static demo adapter does not implement ${path}.`,
    status: 404,
  } satisfies ApiError;
}

function demoAuth(
  name: string,
  email: string,
  preferredLanguage = "en",
  accessibilityPreference?: string,
  role = "RegisteredFan",
): AuthResponse {
  return {
    accessToken: `demo-access-${demoId("token")}`,
    refreshToken: `demo-refresh-${demoId("token")}`,
    accessTokenExpiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    user: {
      id: demoId("user"),
      name,
      email,
      preferredLanguage,
      accessibilityPreference,
      roles: [role],
    },
  };
}

function createDemoIncident(body: unknown): Incident {
  const payload = body as Partial<{ category: string; severity: string; location: string }>;
  const incident: Incident = {
    id: demoId("incident"),
    category: payload.category || "General",
    severity: payload.severity || "Low",
    priority: payload.severity === "Critical" || payload.severity === "High" ? "Critical" : "Normal",
    location: payload.location || "Unknown location",
    status: "Open",
    assignedTeam: payload.category === "Medical" ? "Medical" : "Operations",
    createdAt: new Date().toISOString(),
  };
  const incidents = [incident, ...readDemoList("stadium-ops-demo-incidents", initialDemoIncidents)];
  localStorage.setItem("stadium-ops-demo-incidents", JSON.stringify(incidents));
  return incident;
}

function updateDemoIncident(path: string, body: unknown): Incident {
  const id = path.split("/")[2];
  const payload = body as Partial<{ status: string; assignedTeam: string }>;
  const incidents = readDemoList("stadium-ops-demo-incidents", initialDemoIncidents);
  const next = incidents.map((incident) =>
    incident.id === id
      ? {
          ...incident,
          status: payload.status || incident.status,
          assignedTeam: payload.assignedTeam || incident.assignedTeam,
        }
      : incident,
  );
  localStorage.setItem("stadium-ops-demo-incidents", JSON.stringify(next));
  return next.find((incident) => incident.id === id) || incidents[0];
}

function createDemoNotification(body: unknown): NotificationItem {
  const payload = body as Partial<{ title: string; message: string; type: string; priority: string }>;
  const notification: NotificationItem = {
    id: demoId("notification"),
    title: payload.title || "Demo broadcast",
    message: payload.message || "Static demo broadcast created.",
    type: payload.type || "Operations",
    priority: payload.priority || "Normal",
    isRead: false,
    externalDeliveryStatus: "static-demo",
  };
  const notifications = [notification, ...readDemoList("stadium-ops-demo-notifications", initialDemoNotifications)];
  localStorage.setItem("stadium-ops-demo-notifications", JSON.stringify(notifications));
  return notification;
}

function page<T>(items: T[]): PagedEnvelope<T> {
  return {
    items,
    page: 1,
    pageSize: items.length,
    totalCount: items.length,
    totalPages: 1,
  };
}

function readDemoList<T>(key: string, fallback: T[]): T[] {
  const raw = localStorage.getItem(key);
  if (!raw) {
    return fallback;
  }

  try {
    return JSON.parse(raw) as T[];
  } catch {
    localStorage.removeItem(key);
    return fallback;
  }
}

function demoId(prefix: string) {
  return `${prefix}-${crypto.randomUUID()}`;
}

// ─── Transport demo data ──────────────────────────────────────────────────────

const demoTransportStatuses: TransportStatus[] = [
  {
    id: "transport-metro",
    mode: "Metro",
    provider: "Miami-Dade Transit",
    route: "Metrorail Orange Line → Hard Rock Stadium",
    status: "On Time",
    estimatedDelayMinutes: 0,
    nextArrival: new Date(Date.now() + 8 * 60 * 1000).toISOString(),
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "transport-shuttle",
    mode: "Shuttle",
    provider: "FIFA Fan Shuttle",
    route: "Downtown Miami → Hard Rock Stadium (Express)",
    status: "On Time",
    estimatedDelayMinutes: 0,
    nextArrival: new Date(Date.now() + 12 * 60 * 1000).toISOString(),
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "transport-bus",
    mode: "Bus",
    provider: "Miami-Dade Transit",
    route: "Route 297 → Stadium NW Gate",
    status: "Delayed",
    estimatedDelayMinutes: 7,
    nextArrival: new Date(Date.now() + 22 * 60 * 1000).toISOString(),
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "transport-rideshare",
    mode: "Rideshare",
    provider: "Uber / Lyft",
    route: "Designated pickup Zone D (NW 199th St)",
    status: "High Demand",
    estimatedDelayMinutes: 15,
    nextArrival: new Date(Date.now() + 18 * 60 * 1000).toISOString(),
    lastUpdated: new Date().toISOString(),
  },
  {
    id: "transport-parking",
    mode: "Parking",
    provider: "Hard Rock Stadium Lots",
    route: "Lot 18 (Accessible) — 340/500 spaces available",
    status: "Available",
    estimatedDelayMinutes: 0,
    nextArrival: "",
    lastUpdated: new Date().toISOString(),
  },
];

// ─── Sustainability demo data ─────────────────────────────────────────────────

const demoSustainability: SustainabilityMetric = {
  id: "sustainability-hard-rock",
  stadiumName: "Hard Rock Stadium",
  energyKwh: 12450,
  energyTargetKwh: 15000,
  waterLiters: 8200,
  waterTargetLiters: 12000,
  wasteKg: 3100,
  wasteTargetKg: 4000,
  recyclingRate: 72,
  carbonScore: 84,
  metricDate: new Date().toISOString().split("T")[0],
  ecoTips: [
    "Use the Metro or FIFA Fan Shuttle to reduce carbon emissions by up to 60%",
    "Refill your water bottle at any of the 24 hydration stations throughout the stadium",
    "Sort waste into the color-coded bins — blue for recycling, green for compost, grey for landfill",
    "Digital tickets eliminate 12 tonnes of paper waste per tournament day",
    "LED pitch lighting uses 40% less energy than previous World Cup venues",
  ],
};

// ─── Volunteer demo data ──────────────────────────────────────────────────────

const initialDemoVolunteerTasks: VolunteerTask[] = [
  {
    id: "vol-task-1",
    title: "Gate C Accessibility Assistance",
    location: "Accessible Gate C, Ground Level, North",
    status: "Assigned",
    priority: "High",
    category: "Accessibility",
    startsAt: new Date(Date.now() + 30 * 60 * 1000).toISOString(),
    endsAt: new Date(Date.now() + 150 * 60 * 1000).toISOString(),
    description:
      "Assist wheelchair users and mobility-impaired fans through Accessible Gate C. Coordinate with medical standby team.",
  },
  {
    id: "vol-task-2",
    title: "First Aid Station Standby",
    location: "Medical Station 204, Level 2, West",
    status: "In Progress",
    priority: "High",
    category: "Medical",
    startsAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    endsAt: new Date(Date.now() + 180 * 60 * 1000).toISOString(),
    description: "Support medical staff with fan triage, supply management, and communication with operations center.",
  },
  {
    id: "vol-task-3",
    title: "Recycling Station Monitoring",
    location: "East Food Hall, Level 2, East",
    status: "Assigned",
    priority: "Normal",
    category: "Sustainability",
    startsAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    endsAt: new Date(Date.now() + 240 * 60 * 1000).toISOString(),
    description:
      "Ensure fans use correct waste sorting bins. Track contamination rate and report to sustainability operations.",
  },
  {
    id: "vol-task-4",
    title: "Multilingual Fan Assistance",
    location: "South Gates Plaza, Ground Level",
    status: "Assigned",
    priority: "Normal",
    category: "Fan Experience",
    startsAt: new Date(Date.now() + 45 * 60 * 1000).toISOString(),
    endsAt: new Date(Date.now() + 210 * 60 * 1000).toISOString(),
    description:
      "Help international fans with wayfinding, ticket scanning, and translation support. Use the AI translator for languages you don't speak.",
  },
];

// ─── Demo helpers for new features ────────────────────────────────────────────

function updateDemoVolunteerTask(path: string, body: unknown): VolunteerTask {
  const id = path.split("/").pop()!;
  const payload = body as Partial<{ status: string }>;
  const tasks = readDemoList("stadium-ops-demo-volunteer-tasks", initialDemoVolunteerTasks);
  const next = tasks.map((task) => (task.id === id ? { ...task, status: payload.status || task.status } : task));
  localStorage.setItem("stadium-ops-demo-volunteer-tasks", JSON.stringify(next));
  return next.find((task) => task.id === id) || tasks[0];
}

const demoTranslations: Record<string, Record<string, string>> = {
  es: {
    default:
      "¡Bienvenido al Estadio Hard Rock! El partido de cuartos de final entre Noruega e Inglaterra comienza a las 5:00 PM ET. Siga las señales hacia su sección.",
  },
  fr: {
    default:
      "Bienvenue au Hard Rock Stadium ! Le quart de finale entre la Norvège et l'Angleterre débute à 17h00 ET. Suivez les panneaux vers votre section.",
  },
  ar: {
    default:
      "مرحباً بكم في ملعب هارد روك! تبدأ مباراة ربع النهائي بين النرويج وإنجلترا في الساعة 5:00 مساءً بالتوقيت الشرقي. اتبع اللافتات إلى مقعدك.",
  },
  pt: {
    default:
      "Bem-vindo ao Hard Rock Stadium! A partida das quartas de final entre Noruega e Inglaterra começa às 17:00 ET. Siga as placas até a sua seção.",
  },
  de: {
    default:
      "Willkommen im Hard Rock Stadium! Das Viertelfinalspiel zwischen Norwegen und England beginnt um 17:00 Uhr ET. Folgen Sie den Schildern zu Ihrem Platz.",
  },
  ja: {
    default:
      "ハードロックスタジアムへようこそ！ノルウェー対イングランドの準々決勝は東部時間午後5時に開始します。案内表示に従ってお席へお進みください。",
  },
  ko: {
    default:
      "하드록 스타디움에 오신 것을 환영합니다! 노르웨이 대 잉글랜드 8강전이 동부시간 오후 5시에 시작됩니다. 안내 표지판을 따라 좌석으로 이동하세요.",
  },
  zh: {
    default: "欢迎来到硬石体育场！挪威对阵英格兰的四分之一决赛将于东部时间下午5:00开始。请按照指示牌前往您的座位区域。",
  },
};

function demoTranslate(body: unknown): TranslationResponse {
  const payload = body as Partial<{ text: string; fromLanguage: string; toLanguage: string }>;
  const target = payload.toLanguage || "es";
  const translations = demoTranslations[target];
  const translated = translations?.default || `[${target}] ${payload.text || "Translation unavailable"}`;
  return {
    originalText: payload.text || "",
    translatedText: translated,
    fromLanguage: payload.fromLanguage || "en",
    toLanguage: target,
    model: "static-demo",
  };
}

function demoLiveAlerts(): LiveAlert[] {
  const now = Date.now();
  return [
    {
      id: "alert-1",
      type: "Crowd",
      message: "East Food Hall density at 82% — consider North Concourse alternatives",
      timestamp: new Date(now - 2 * 60 * 1000).toISOString(),
      severity: "warn",
    },
    {
      id: "alert-2",
      type: "Transport",
      message: "Bus Route 297 delayed 7 minutes — Metro running on schedule",
      timestamp: new Date(now - 5 * 60 * 1000).toISOString(),
      severity: "info",
    },
    {
      id: "alert-3",
      type: "Sustainability",
      message: "Recycling rate trending up: 72% — target 75% by end of match",
      timestamp: new Date(now - 8 * 60 * 1000).toISOString(),
      severity: "ok",
    },
    {
      id: "alert-4",
      type: "Operations",
      message: "Quarter-Final Norway vs England — gates open in 2 hours",
      timestamp: new Date(now - 12 * 60 * 1000).toISOString(),
      severity: "info",
    },
  ];
}

type DemoUserEntry = {
  name: string;
  role: string;
  preferredLanguage: string;
  accessibilityPreference?: string;
};

function readDemoRegistry(): Record<string, DemoUserEntry> {
  const raw = localStorage.getItem("stadium-ops-demo-users");
  if (!raw) return {};
  try {
    return JSON.parse(raw) as Record<string, DemoUserEntry>;
  } catch {
    localStorage.removeItem("stadium-ops-demo-users");
    return {};
  }
}
