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
      auth: false
    });
  }

  async login(payload: { email: string; password: string; deviceName: string }) {
    return this.request<AuthResponse>("/auth/login", {
      method: "POST",
      body: payload,
      auth: false
    });
  }

  async logout(refreshToken: string) {
    return this.request<{ message: string }>("/auth/logout", {
      method: "POST",
      body: { refreshToken }
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
      body: payload
    });
  }

  async aiChat(payload: { prompt: string; context?: string }) {
    return this.request<AiChatResponse>("/ai/chat", {
      method: "POST",
      body: payload
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

  async createIncident(payload: {
    category: string;
    severity: string;
    location: string;
    description: string;
  }) {
    return this.request<Incident>("/incidents", {
      method: "POST",
      body: payload
    });
  }

  async updateIncidentStatus(id: string, status: string, assignedTeam?: string) {
    return this.request<Incident>(`/incidents/${id}/status`, {
      method: "PATCH",
      body: { status, assignedTeam }
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

  async broadcast(payload: {
    title: string;
    message: string;
    type: string;
    priority: string;
    deviceToken?: string;
  }) {
    return this.request<NotificationItem>("/notifications/broadcast", {
      method: "POST",
      body: payload
    });
  }

  private async request<T>(
    path: string,
    options: {
      method?: string;
      body?: unknown;
      auth?: boolean;
    } = {}
  ): Promise<T> {
    const headers: Record<string, string> = {
      Accept: "application/json",
      "X-Correlation-ID": crypto.randomUUID()
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
          status: 401
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
        body: options.body === undefined ? undefined : JSON.stringify(options.body)
      });
    } catch {
      throw {
        title: "Network error",
        detail: `Could not reach the API at ${API_BASE_URL}. Start the ASP.NET API or update VITE_API_BASE_URL.`,
        status: 0
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
          status: response.status
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
      correlationId: asString(data.correlationId)
    };
  }

  return {
    title: `HTTP ${status}`,
    detail: "The API request failed.",
    status
  };
}

function asString(value: unknown) {
  return typeof value === "string" ? value : undefined;
}

const demoStadium: Stadium = {
  id: "3da80b77-2580-4dc9-9df1-3f478f86ef49",
  name: "MetLife Stadium",
  city: "East Rutherford",
  country: "United States",
  capacity: 82500,
  latitude: 40.8135,
  longitude: -74.0745
};

const demoPois: PointOfInterest[] = [
  {
    id: "poi-accessible-gate-c",
    name: "Accessible Gate C",
    category: "Entrance",
    level: "Ground",
    zone: "North",
    isAccessible: true,
    estimatedWaitMinutes: 6
  },
  {
    id: "poi-global-food-hall",
    name: "Global Food Hall",
    category: "Food",
    level: "2",
    zone: "East",
    isAccessible: true,
    estimatedWaitMinutes: 12
  },
  {
    id: "poi-medical-204",
    name: "Medical Station 204",
    category: "Medical",
    level: "2",
    zone: "West",
    isAccessible: true,
    estimatedWaitMinutes: 0
  }
];

const demoAgents: AiAgent[] = [
  {
    key: "fan-assistant",
    displayName: "Fan Assistant Agent",
    description: "Answers stadium, match-day, food, merchandise, and lost-and-found questions.",
    intents: ["General", "FanExperience", "Food"],
    responsibilities: ["Stadium FAQs", "Match information", "Food recommendations"],
    requiresOperationalRole: false,
    safetyCritical: false
  },
  {
    key: "smart-navigation",
    displayName: "Smart Navigation Agent",
    description: "Provides indoor, outdoor, accessible, and crowd-aware routing guidance.",
    intents: ["Navigation", "Route", "SeatFinding"],
    responsibilities: ["Accessible routing", "Crowd-aware routes", "Route recalculation"],
    requiresOperationalRole: false,
    safetyCritical: false
  },
  {
    key: "accessibility-assistant",
    displayName: "Accessibility Assistant",
    description: "Prioritizes wheelchair-safe routes, accessible entrances, elevator guidance, and assistive experiences.",
    intents: ["Accessibility", "AccessibleRoute"],
    responsibilities: ["Wheelchair routing", "Accessible entrances", "Elevator guidance"],
    requiresOperationalRole: false,
    safetyCritical: false
  },
  {
    key: "operations-intelligence",
    displayName: "Operations Intelligence Agent",
    description: "Summarizes operational conditions, incidents, resource needs, and staff allocation.",
    intents: ["Operations", "IncidentSummary", "Staffing"],
    responsibilities: ["Operational summaries", "Incident prioritization", "Staff allocation"],
    requiresOperationalRole: true,
    safetyCritical: true
  },
  {
    key: "emergency-response",
    displayName: "Emergency Response Agent",
    description: "Supports emergency guidance and responder escalation without replacing protocols.",
    intents: ["Emergency", "Medical", "Security", "Evacuation"],
    responsibilities: ["Medical guidance", "Security escalation", "Evacuation guidance"],
    requiresOperationalRole: true,
    safetyCritical: true
  }
];

const demoKnowledge: AiKnowledgeDocument[] = [
  {
    id: "knowledge-accessibility",
    category: "Accessibility",
    title: "Demo Accessibility Routing Guide",
    sourceType: "StaticDemo",
    contentSummary: "Use accessible gates, elevators, and staff checkpoints for wheelchair and mobility-support routes.",
    language: "en",
    isApproved: true
  },
  {
    id: "knowledge-emergency",
    category: "Emergency Procedures",
    title: "Demo Emergency Escalation Guidance",
    sourceType: "StaticDemo",
    contentSummary: "AI guidance is advisory. Safety issues must be escalated to venue responders and official protocols.",
    language: "en",
    isApproved: true
  },
  {
    id: "knowledge-transport",
    category: "Transportation",
    title: "Demo Transportation Assistance Guide",
    sourceType: "StaticDemo",
    contentSummary: "Recommend shuttles, public transit, and accessible pickup areas when configured data is available.",
    language: "en",
    isApproved: true
  }
];

const demoCrowdZones: CrowdZone[] = [
  {
    id: "crowd-north-concourse",
    stadiumName: demoStadium.name,
    name: "North Concourse",
    currentDensity: 62,
    maximumCapacity: 100,
    status: "Elevated",
    lastUpdated: new Date().toISOString()
  },
  {
    id: "crowd-east-food-hall",
    stadiumName: demoStadium.name,
    name: "East Food Hall",
    currentDensity: 78,
    maximumCapacity: 100,
    status: "Congested",
    lastUpdated: new Date().toISOString()
  }
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
    createdAt: new Date().toISOString()
  }
];

const initialDemoNotifications: NotificationItem[] = [
  {
    id: "notification-demo-1",
    title: "Welcome to Stadium Ops",
    message: "Static demo mode is active until a deployed API URL is configured.",
    type: "System",
    priority: "Normal",
    isRead: false,
    externalDeliveryStatus: "static-demo"
  }
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
    return demoAuth(payload.name || "Demo Fan", payload.email || "fan@example.com", payload.preferredLanguage, payload.accessibilityPreference, payload.requestedRole) as T;
  }

  if (path === "/auth/login") {
    const payload = body as Partial<{ email: string }>;
    return demoAuth("Demo Operator", payload.email || "operator@example.com", "en", "Wheelchair route", "RegisteredFan") as T;
  }

  if (path === "/auth/logout") {
    return { message: "Demo session cleared." } as T;
  }

  if (path === "/auth/me") {
    return demoAuth("Demo Fan", "fan@example.com", "en", undefined, "RegisteredFan").user as T;
  }

  if (path === "/matches/today") {
    return {
      id: "match-demo-1",
      homeTeam: "USA",
      awayTeam: "Canada",
      startsAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      stage: "Group Stage",
      status: "Scheduled",
      stadium: demoStadium
    } as T;
  }

  if (path === "/stadiums") {
    return page([demoStadium]) as T;
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
      recommendationNotice: "Static demo recommendation. Connect the ASP.NET API for live routing."
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
      agentName: emergency ? "Emergency Response Agent" : accessibility ? "Accessibility Assistant" : "Fan Assistant Agent",
      confidenceScore: emergency ? 0.94 : 0.82,
      escalationRecommended: emergency,
      sources: demoKnowledge.map((document) => document.category)
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
      stadiumCount: 1,
      matchCount: 1,
      openIncidentCount: incidents.filter((incident) => incident.status !== "Resolved").length,
      criticalIncidentCount: incidents.filter((incident) => incident.priority === "Critical").length,
      congestedZoneCount: demoCrowdZones.filter((zone) => zone.status === "Congested").length,
      activeVolunteerTasks: 8,
      averageSustainabilityScore: 82,
      aiInsightReadiness: ["Static demo mode active", "Deploy ASP.NET API for live integrations"]
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

  throw {
    title: "Static demo route missing",
    detail: `The static demo adapter does not implement ${path}.`,
    status: 404
  } satisfies ApiError;
}

function demoAuth(
  name: string,
  email: string,
  preferredLanguage = "en",
  accessibilityPreference?: string,
  role = "RegisteredFan"
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
      roles: [role]
    }
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
    createdAt: new Date().toISOString()
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
      ? { ...incident, status: payload.status || incident.status, assignedTeam: payload.assignedTeam || incident.assignedTeam }
      : incident
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
    externalDeliveryStatus: "static-demo"
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
    totalPages: 1
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
