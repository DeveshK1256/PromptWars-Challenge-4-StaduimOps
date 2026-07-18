import { Bell } from "lucide-react";
import { FormEvent, useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, NotificationItem } from "../services/api";
import type { AsyncStatus } from "../types";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

export function NotificationConsole({ client }: { client: ApiClient }) {
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
        deviceToken: String(data.get("deviceToken") ?? "") || undefined,
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
                <span className={item.priority === "Critical" ? "status-pill danger" : "status-pill"}>
                  {item.priority}
                </span>
              </div>
            ))}
          </div>
        </div>
      </article>
    </section>
  );
}
