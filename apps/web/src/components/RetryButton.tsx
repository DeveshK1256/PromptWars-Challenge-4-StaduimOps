import { RefreshCw } from "lucide-react";

export function RetryButton({ onRetry, loading }: { onRetry: () => void; loading: boolean }) {
  return (
    <button className="secondary-action compact" type="button" onClick={onRetry} disabled={loading}>
      <RefreshCw aria-hidden="true" className={loading ? "spin" : ""} />
      Refresh
    </button>
  );
}
