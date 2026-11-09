# School Admission System

A Docker-first monorepo for a first-come-first-served school admission workflow.

## Quick start

1. Copy `.env.example` to `.env`.
2. Run `docker compose up --build`.
3. Open the frontend at `http://localhost:3000`.
4. Open the API at `http://localhost:8080`.
5. Open Swagger at `http://localhost:8080/swagger` when running in Development.

No local .NET, Node.js, or PostgreSQL installation should be required to run the application. Docker and Docker Compose are required.

## Repository map

- `backend/`: ASP.NET Core Web API, DDD, Clean Architecture, and automated tests.
- `frontend/`: React + TypeScript application for school administration and parents.
- `docs/`: requirements, architecture, contracts, implementation instructions, and acceptance criteria.
- `infrastructure/`: Docker build assets and database support files.
- `docker-compose.yml`: starts PostgreSQL, API, and frontend together.

## Required workflow

Read `docs/19-ai-agent-rules.md` first, then `docs/00-project-overview.md`, `docs/02-assumptions.md`, and the architecture/API documents. Implement in the order defined by `docs/20-implementation-plan.md`.

## Important assignment constraints

- Admission is first-come-first-served; there is no lottery.
- Capacity is per class-and-shift admission unit.
- A seat hold expires after `HOLD_DURATION_SECONDS` (default 600).
- The API must never allocate more active holds plus confirmed seats than capacity.
- The frontend must use the API; it must not connect directly to PostgreSQL.
- Student identity is supplied through `X-Student-Id`; the assignment does not require a login screen.
- Payment is simulated: clicking the payment action means payment succeeds, subject to the hold still being valid.
- Data must survive API/container restarts and `docker compose down` followed by `docker compose up`. Do not use `docker compose down -v` if you want to preserve the database volume.

## Commands

```bash
docker compose up --build
docker compose down
docker compose logs -f api
docker compose logs -f frontend
docker compose exec postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
```

Run backend and frontend tests using the commands documented in `docs/16-testing-strategy.md`.
