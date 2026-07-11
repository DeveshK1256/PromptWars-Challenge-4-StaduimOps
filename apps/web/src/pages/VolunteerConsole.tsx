import { HandHeart, Play, CheckCircle2, Clock } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import type { ApiClient, ApiError, VolunteerTask } from "../services/api";
import type { AsyncStatus } from "../types";
import { RetryButton } from "../components/RetryButton";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

function priorityTone(priority: string) {
  if (priority === "High" || priority === "Critical") return "warn";
  return "";
}

function statusLabel(status: string) {
  switch (status) {
    case "Assigned":
      return "Assigned";
    case "In Progress":
      return "Active";
    case "Completed":
      return "Done";
    default:
      return status;
  }
}

function statusTone(status: string) {
  if (status === "Completed") return "ok";
  if (status === "In Progress") return "warn";
  return "";
}

export function VolunteerConsole({ client }: { client: ApiClient }) {
  const [tasks, setTasks] = useState<VolunteerTask[]>([]);
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);

  const load = useCallback(async () => {
    setStatus("loading");
    setError(null);
    try {
      setTasks(await client.volunteerTasks());
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void load();
  }, [load]);

  async function updateTask(id: string, newStatus: string) {
    setStatus("loading");
    setError(null);
    try {
      await client.updateVolunteerTask(id, newStatus);
      await load();
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  const activeTasks = tasks.filter((t) => t.status !== "Completed");
  const completedTasks = tasks.filter((t) => t.status === "Completed");

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<HandHeart aria-hidden="true" />}
        title="Volunteer hub"
        description="Task assignments, shift management, and operational updates for tournament volunteers."
        action={<RetryButton onRetry={load} loading={status === "loading"} />}
      />
      <StatusNotice status={status} error={error} success="Volunteer tasks loaded." />

      <div className="metric-grid">
        <article className="metric">
          <span>Total tasks</span>
          <strong>{tasks.length}</strong>
        </article>
        <article className="metric warn">
          <span>Active</span>
          <strong>{tasks.filter((t) => t.status === "In Progress").length}</strong>
        </article>
        <article className="metric">
          <span>Pending</span>
          <strong>{tasks.filter((t) => t.status === "Assigned").length}</strong>
        </article>
        <article className="metric ok">
          <span>Completed</span>
          <strong>{completedTasks.length}</strong>
        </article>
      </div>

      <article className="panel">
        <h2>Active &amp; upcoming tasks</h2>
        <div className="list-stack">
          {activeTasks.length === 0 && <p className="muted">No active tasks. All assignments are completed.</p>}
          {activeTasks.map((task) => (
            <div className="row-card volunteer-task" key={task.id}>
              <div>
                <strong>{task.title}</strong>
                <span>{task.location}</span>
                <span className="muted">
                  <Clock aria-hidden="true" style={{ width: 12, height: 12 }} />{" "}
                  {new Date(task.startsAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })} –{" "}
                  {new Date(task.endsAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                </span>
                <span className="muted">{task.description}</span>
              </div>
              <div className="volunteer-actions">
                <span className={`status-pill ${priorityTone(task.priority)}`}>{task.priority}</span>
                <span className={`status-pill ${statusTone(task.status)}`}>{statusLabel(task.status)}</span>
                {task.status === "Assigned" && (
                  <button
                    className="secondary-action compact"
                    type="button"
                    onClick={() => void updateTask(task.id, "In Progress")}
                    disabled={status === "loading"}
                  >
                    <Play aria-hidden="true" style={{ width: 14, height: 14 }} />
                    Start
                  </button>
                )}
                {task.status === "In Progress" && (
                  <button
                    className="secondary-action compact"
                    type="button"
                    onClick={() => void updateTask(task.id, "Completed")}
                    disabled={status === "loading"}
                  >
                    <CheckCircle2 aria-hidden="true" style={{ width: 14, height: 14 }} />
                    Complete
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </article>

      {completedTasks.length > 0 && (
        <article className="panel">
          <h2>Completed tasks</h2>
          <div className="list-stack">
            {completedTasks.map((task) => (
              <div className="row-card" key={task.id} style={{ opacity: 0.7 }}>
                <div>
                  <strong>{task.title}</strong>
                  <span>{task.location}</span>
                </div>
                <span className="status-pill ok">Done</span>
              </div>
            ))}
          </div>
        </article>
      )}
    </section>
  );
}
