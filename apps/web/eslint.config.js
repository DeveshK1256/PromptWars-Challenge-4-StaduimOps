// eslint.config.js — flat config for TypeScript + React
import js from "@eslint/js";
import tseslint from "typescript-eslint";
import reactHooks from "eslint-plugin-react-hooks";

export default tseslint.config(
  // Base recommended rules
  js.configs.recommended,

  // TypeScript rules (syntax-only, no type-aware rules that require TS project service)
  ...tseslint.configs.recommended,

  // React Hooks rules
  {
    plugins: {
      "react-hooks": reactHooks
    },
    rules: {
      ...reactHooks.configs.recommended.rules
    }
  },

  // Project-specific overrides
  {
    rules: {
      // Allow explicit `any` with a warning during migration
      "@typescript-eslint/no-explicit-any": "warn",
      // Disallow unused variables except those prefixed with _
      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_", varsIgnorePattern: "^_" }
      ],
      // Allow empty catch blocks (used intentionally in a few places)
      "no-empty": ["error", { allowEmptyCatch: true }]
    }
  },

  // Files to ignore
  {
    ignores: ["dist/**", "node_modules/**", "*.config.{js,cjs,mjs,ts}", "src/vite-env.d.ts"]
  }
);
