# 12 — Frontend Architecture

## Stack

- React 18+
- TypeScript strict mode
- Vite
- React Router
- TanStack Query for server state
- Axios or typed `fetch` wrapper
- React Hook Form + Zod for forms
- Tailwind CSS (or another consistent component system)

## Feature folders

```text
frontend/src/
  app/
    App.tsx
    router.tsx
    providers/
  features/
    admission-classes/
      api/
      components/
      hooks/
      pages/
      schemas/
      types/
    student-holds/
      api/
      components/
      hooks/
      pages/
      schemas/
      types/
    admin/
      components/
      pages/
  components/
    ui/
    layout/
  lib/
    api-client.ts
    query-client.ts
  hooks/
  types/
  utils/
  styles/
```

## Routes

- `/` — parent admission page.
- `/my-applications` — applications for the currently entered student ID.
- `/admin` — school admin class creation and seat overview.
- `*` — not-found page.

## Student ID persistence

Store the current student ID in `sessionStorage` or `localStorage` after explicit user entry. Explain that this is a convenience identifier, not secure authentication. Send it as `X-Student-Id` for all student-scoped endpoints. Never store secrets because there are no credentials in this assignment.

## API configuration

Use `VITE_API_BASE_URL` with a development default of `http://localhost:8080`. Do not hard-code API URLs in feature components. Ensure the Docker deployment can configure the URL for the browser; a container-only hostname such as `http://api:8080` is not reachable from a user's browser.

## Data freshness

- Invalidate class list/detail queries after hold, confirm, or cancel.
- Poll the student's hold list and class counts at a modest interval while a hold is active (e.g. every 5 seconds), or use a deliberate refresh control plus local expiry countdown. Prefer polling for this small assignment.
- Use server timestamps as source of truth; browser countdown is informational only.
- On tab focus, refetch current holds and class counts.

## Component rules

- UI components do not call raw URLs.
- Use typed request/response models matching `08-api-contract.md`.
- Handle loading, empty, error, success, and conflict states.
- Disable confirm when the hold is expired or not `HELD`.
