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

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000/api/v1";

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
