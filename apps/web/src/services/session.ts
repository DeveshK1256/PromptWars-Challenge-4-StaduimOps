import type { AuthResponse, UserProfile } from "./api";

const STORAGE_KEY = "stadium-ops-session";

export type StoredSession = {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserProfile;
  language: string;
  theme: "standard" | "contrast";
};

export function loadSession(): StoredSession | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as StoredSession;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

export function saveSession(auth: AuthResponse, previous?: StoredSession | null) {
  const session: StoredSession = {
    accessToken: auth.accessToken,
    refreshToken: auth.refreshToken,
    accessTokenExpiresAt: auth.accessTokenExpiresAt,
    user: auth.user,
    language: previous?.language ?? auth.user.preferredLanguage ?? "en",
    theme: previous?.theme ?? "standard"
  };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  return session;
}

export function replaceSession(session: StoredSession) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(STORAGE_KEY);
}
