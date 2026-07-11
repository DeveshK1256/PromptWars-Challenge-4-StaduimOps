import { AlertTriangle, CheckCircle2, RefreshCw } from "lucide-react";
import type { ApiError } from "../services/api";
import type { AsyncStatus } from "../types";

export function StatusNotice({
  status,
  error,
  success,
}: {
  status: AsyncStatus;
  error: ApiError | null;
  success: string;
}) {
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
