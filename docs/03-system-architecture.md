# 03 — System Architecture

## Context

```text
Browser
  |
  | HTTP / JSON
  v
React + TypeScript (Nginx container, :3000)
  |
  | configurable VITE_API_BASE_URL
  v
ASP.NET Core Web API (:8080)
  |
  | Application use cases
  v
Domain model (AdmissionClass, SeatHold)
  |
  | Infrastructure abstractions
  v
EF Core / PostgreSQL (:5432)
```

## Runtime services

- `frontend`: static React build served by Nginx.
- `api`: ASP.NET Core Web API.
- `postgres`: PostgreSQL with a named persistent volume.
- Optional `migrator`: may be implemented as an API startup migration step for this assignment, or as a one-shot service. Use one migration strategy consistently.

## Architecture rules

- Browser communicates only with the API.
- API is the only application component that accesses PostgreSQL.
- Domain rules live in the Domain project.
- Application coordinates use cases and transactions through abstractions.
- Infrastructure implements persistence, clock, and other external concerns.
- API project is the composition root and HTTP adapter.
- Frontend features call typed API clients; components do not construct ad hoc URLs.

## Deployment topology

Compose network service names are used internally:
- API connects to database host `postgres`, not `localhost`.
- Frontend browser requests use the host-accessible API URL, normally `http://localhost:8080`.
- `localhost` inside a container refers to that container, not another service.

## Reliability

- PostgreSQL uses a named volume.
- API health endpoint: `GET /health`.
- Compose health checks gate API startup on database readiness.
- API should retry transient database startup failures or rely on health-gated startup.
