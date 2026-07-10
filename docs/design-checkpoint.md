# Design Checkpoint

Image generation was unavailable in this Codex session, so the first UI checkpoint is implemented as a code-native design system and screen inventory.

## Visual Direction

- Product type: dense enterprise operations platform, not a marketing landing page.
- First authenticated screen: usable dashboard shell with role navigation and operational workflows.
- Palette: neutral operational background, white surfaces, stadium teal as primary action, blue for maps/navigation, gold for warnings, red for critical incidents.
- Container model: persistent sidebar, topbar identity strip, bounded panels for workflows, metric tiles only for dashboard KPIs.
- Radius: `8px` maximum for app panels and controls.
- Typography: system UI with explicit control, label, heading, and metric sizing.
- Accessibility: skip link, visible focus, keyboard-ready buttons/forms, semantic headings, high-contrast mode, reduced-motion handling.

## Screen Inventory

- Authentication: login/register tabs, role selection for permitted self-registration roles, validation, loading/success/error states.
- Fan console: match card, stadium services, POI list, Google Maps readiness, route request with accessibility option.
- Command center: role-protected operations metrics, crowd zones, integration readiness.
- AI assistant: Vertex AI-backed prompt workflow; missing configuration appears as a 503 service state.
- Incidents: authenticated reporting, role-protected triage, status update.
- Notifications: in-app persisted broadcast; optional Firebase delivery when configured.

## Interaction Rules

- Every button either changes state, submits an API request, refreshes data, signs out, or opens an external Google Maps URL.
- Seed data is development/demo seed data and must not be represented as live tournament data.
- AI output is labeled by model/intent/tokens when returned and never replaces emergency protocols.
