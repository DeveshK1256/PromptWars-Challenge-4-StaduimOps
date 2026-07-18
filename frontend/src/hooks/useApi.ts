const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5082/api/v1";

function getToken(): string | null {
  return localStorage.getItem("access_token");
}

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getToken();
  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options?.headers,
    },
  });
  if (!res.ok) throw new Error(`API error ${res.status}`);
  const json = await res.json();
  return json.data ?? json;
}

export const api = {
  getStadiums: () => apiFetch<any>("/stadiums"),
  getTodayMatch: () => apiFetch<any>("/matches/today"),
  getStadiumPois: (id: string) => apiFetch<any>(`/stadiums/${id}/pois`),
  getIncidents: (status?: string) => apiFetch<any>(`/incidents${status ? `?status=${status}` : ""}`),
  createIncident: (body: any) => apiFetch<any>("/incidents", { method: "POST", body: JSON.stringify(body) }),
  getNotifications: () => apiFetch<any>("/notifications"),
  markNotificationRead: (id: string) => apiFetch<any>(`/notifications/${id}/read`, { method: "PATCH", body: JSON.stringify({}) }),
  getAiAgents: () => apiFetch<any>("/ai/agents"),
  aiChat: (prompt: string, context?: string) => apiFetch<any>("/ai/chat", { method: "POST", body: JSON.stringify({ prompt, context }) }),
  getAiConversations: () => apiFetch<any>("/ai/conversations"),
  getOperations: () => apiFetch<any>("/operations"),
};
