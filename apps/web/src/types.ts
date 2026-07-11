// Shared primitive types and constants used across pages and components.

/** The top-level view key used to switch between dashboard panels. */
export type ViewKey = "fan" | "operations" | "ai" | "incidents" | "notifications";

/** Represents the lifecycle state of any async API call. */
export type AsyncStatus = "idle" | "loading" | "success" | "error";

/** Role options presented in the registration form. */
export const roleOptions = ["RegisteredFan", "Volunteer"];
