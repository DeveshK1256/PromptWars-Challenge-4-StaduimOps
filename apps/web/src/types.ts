// Shared primitive types and constants used across pages and components.

/** The top-level view key used to switch between dashboard panels. */
export type ViewKey =
  "fan" | "operations" | "ai" | "incidents" | "notifications" | "transport" | "sustainability" | "volunteer";

/** Represents the lifecycle state of any async API call. */
export type AsyncStatus = "idle" | "loading" | "success" | "error";

/** Role options presented in the registration form. */
export const roleOptions = ["RegisteredFan", "Volunteer"];

/** Languages supported for multilingual assistance (FIFA WC 2026). */
export const supportedLanguages = [
  { code: "en", label: "English" },
  { code: "es", label: "Español" },
  { code: "fr", label: "Français" },
  { code: "ar", label: "العربية" },
  { code: "pt", label: "Português" },
  { code: "de", label: "Deutsch" },
  { code: "ja", label: "日本語" },
  { code: "ko", label: "한국어" },
  { code: "zh", label: "中文" },
];
