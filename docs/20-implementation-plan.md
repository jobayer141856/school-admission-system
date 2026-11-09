# 20 — Implementation Plan

Implement in vertical slices. Keep the application runnable after each phase.

## Phase 0 — Repository foundation

- Create monorepo folders and solution.
- Add root README, `.gitignore`, `.dockerignore`, `.env.example`.
- Add Docker Compose skeleton.
- Add CI workflow skeleton.

Exit criteria: repository structure is clear and Docker plan is documented.

## Phase 1 — Backend foundation

- Create Domain, Application, Infrastructure, API projects.
- Configure dependency direction and DI.
- Add PostgreSQL EF Core DbContext.
- Add migrations and health endpoint.
- Add ProblemDetails middleware and Swagger.

Exit criteria: API starts and connects to PostgreSQL in Compose.

## Phase 2 — Domain and persistence

- Implement AdmissionClass and SeatHold aggregates.
- Implement status transitions and validation.
- Configure tables, indexes, and migrations.
- Add fake-clock domain tests.

Exit criteria: domain tests pass and schema is created.

## Phase 3 — Read/create admission classes

- Implement `POST /classes`.
- Implement `GET /classes`.
- Implement `GET /classes/{classId}`.
- Calculate counts from persisted records.

Exit criteria: API contract integration tests pass.

## Phase 4 — Hold lifecycle and concurrency

- Implement create hold with transaction and locking.
- Implement get hold and student hold list.
- Implement confirm and cancel.
- Implement expiry processing.
- Add PostgreSQL concurrency tests.

Exit criteria: overselling test passes; duplicate confirmation is idempotent.

## Phase 5 — Frontend foundation

- Create Vite React TypeScript app.
- Configure router, API client, query client, styling.
- Add parent identity entry and class list.

Exit criteria: frontend displays real API data in Docker.

## Phase 6 — Parent workflow

- Add create hold, my applications, countdown, simulated payment, cancellation.
- Add refresh recovery and polling.
- Add loading/error/empty/conflict states.

Exit criteria: parent completes entire workflow through UI.

## Phase 7 — Admin workflow

- Add `/admin` class creation form and class overview.
- Add validation and success/error feedback.

Exit criteria: admin can create admission units through UI.

## Phase 8 — Quality and submission

- Run all tests and builds.
- Verify clean Docker startup and persistence.
- Verify required API routes and headers.
- Complete `ASSUMPTIONS.md` and `AI_USAGE.md` (or keep these as root files if required by evaluator).
- Add screenshots and concise setup instructions.
- Review public GitHub repository for secrets and accidental artifacts.

Exit criteria: all checks in `21-acceptance-criteria.md` pass.
