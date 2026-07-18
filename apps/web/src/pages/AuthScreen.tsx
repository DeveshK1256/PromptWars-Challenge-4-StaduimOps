import { CheckCircle2, RefreshCw } from "lucide-react";
import { FormEvent, useState, useMemo } from "react";
import type { ApiError } from "../services/api";
import { ApiClient, apiMode } from "../services/api";
import { saveSession } from "../services/session";
import type { AsyncStatus } from "../types";
import { roleOptions } from "../types";
import { AnimatedBackground } from "../components/AnimatedBackground";
import { StatusNotice } from "../components/StatusNotice";

export function AuthScreen({
  onAuthenticated,
}: {
  onAuthenticated: (auth: Parameters<typeof saveSession>[0]) => void;
}) {
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
              requestedRole: String(data.get("requestedRole") ?? "RegisteredFan"),
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
      <AnimatedBackground />
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
          {apiMode === "demo"
            ? "Static demo data is active for this Netlify deployment."
            : "Connects to the ASP.NET Core API. No backend success is simulated."}
        </p>
        {apiMode === "demo" && (
          <div className="status-line" role="status">
            <CheckCircle2 aria-hidden="true" />
            <div>
              <strong>Static demo mode</strong>
              <span>Set VITE_API_BASE_URL to a deployed ASP.NET API to use live backend workflows.</span>
            </div>
          </div>
        )}

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
            {status === "loading" ? (
              <RefreshCw aria-hidden="true" className="spin" />
            ) : (
              <CheckCircle2 aria-hidden="true" />
            )}
            {mode === "login" ? "Sign in" : "Create account"}
          </button>
        </form>
        <StatusNotice status={status} error={error} success="Authentication completed." />
      </section>
    </main>
  );
}
