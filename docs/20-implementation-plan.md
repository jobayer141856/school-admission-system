# 20 — Implementation Plan

Implement in vertical slices. Keep the application runnable after each phase.

---

## Phase 0 — Repository foundation `[COMPLETED]`

- [x] Create monorepo folders and solution (`backend/SchoolAdmission.sln`).
- [x] Add root README, `.gitignore`, `.dockerignore`, `.env.example`.
- [x] Add Docker Compose skeleton (`docker-compose.yml`).
- [x] Add CI workflow skeleton (`.github/workflows/ci.yml`).

**Exit criteria**: Repository structure is established, .NET 10 target configured, and Docker Compose definition verified.

---

## Phase 1 — Backend foundation `[COMPLETED]`

- [x] Create Domain, Application, Infrastructure, API, and UnitTests projects targeting **.NET 10 (`net10.0`)**.
- [x] Configure Clean Architecture dependency direction and DI extensions:
  - `Domain`: zero dependencies, pure business abstractions (`Entity<TId>`).
  - `Application`: depends on Domain; defines `IClock` abstraction and DI extension.
  - `Infrastructure`: depends on Application and Domain; sets up `SchoolAdmissionDbContext` with PostgreSQL Npgsql, system clock (`SystemClock`), and DI extension.
  - `API`: depends on Application and Infrastructure; acts as composition root, registers CORS, ProblemDetails, HealthChecks, and OpenAPI/Swagger.
- [x] Add PostgreSQL EF Core DbContext (`SchoolAdmissionDbContext`).
- [x] Add database health check and health endpoint at `GET /health`.
- [x] Add RFC 7807 `ProblemDetailsExceptionHandler` middleware.
- [x] Add OpenAPI and Swagger UI at `GET /swagger` and Scalar documentation at `GET /scalar/v1`.
- [x] Add multi-stage `backend/Dockerfile` with .NET 10 SDK and ASP.NET runtime images.
- [x] Add unit tests for Clean Architecture boundaries, `IClock`, and ProblemDetails status code mapping.

**Exit criteria**: Backend builds cleanly, all automated tests pass, and API starts on port 8080 with `/health` and `/swagger` responsive.

---

## Phase 2 — Domain and persistence `[COMPLETED]`

- [x] Implement `AdmissionClass` aggregate root (`Name`, `TotalSeats`, `CreatedAtUtc`, validations for non-empty bounded name and positive capacity).
- [x] Implement `SeatHold` aggregate root (`AdmissionClassId`, `StudentId`, `Status`, `CreatedAtUtc`, `ExpiresAtUtc`, `ConfirmedAtUtc`, `CancelledAtUtc`).
- [x] Implement `StudentId` value object (trimmed, non-empty, max 64 chars) and `SeatHoldStatus` enum (`HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`).
- [x] Implement domain state transitions and invariants:
  - `Create(...)` sets status `HELD` and expiry based on duration.
  - `Confirm(now)` is idempotent for `CONFIRMED`, rejects expired/cancelled, sets `ConfirmedAtUtc`.
  - `Cancel(now)` is idempotent for `CANCELLED`, rejects confirmed/expired, sets `CancelledAtUtc`.
  - `Expire(now)` transitions `HELD` to `EXPIRED` if past expiry.
- [x] Configure EF Core Fluent API mappings in `backend/src/SchoolAdmission.Infrastructure/Persistence/Configurations/`:
  - Table `admission_classes` with `CHECK (total_seats > 0)`.
  - Table `seat_holds` with indexes `IX_seat_holds_class_status_expires` and `IX_seat_holds_student_status_expires`.
- [x] Generate initial EF Core migration `InitialCreate`.
- [x] Add unit tests for domain entities and value objects (35/35 tests passing).

**Exit criteria**: All domain unit tests pass (35/35), database schema and migrations are generated matching `docs/07-database-design.md`.

---

## Phase 3 — Read/create admission classes `[COMPLETED]`

- [x] Implement ID formatting helper (`c_<guid>` and `h_<guid>`) in `backend/src/SchoolAdmission.Application/Common/IdHelper.cs`.
- [x] Implement request/response DTOs: `CreateAdmissionClassRequest`, `AdmissionClassResponse`, and `AdmissionClassDetailResponse`.
- [x] Implement `IAdmissionClassRepository` abstraction in Application and EF Core implementation in Infrastructure.
- [x] Implement dynamic seat count calculations:
  - `heldSeats`: count where `status = 'HELD' AND expires_at_utc > now`
  - `confirmedSeats`: count where `status = 'CONFIRMED'`
  - `availableSeats`: `totalSeats - heldSeats - confirmedSeats`
- [x] Implement application use cases:
  - `CreateAdmissionClassHandler`: validates input, creates aggregate, persists, returns `c_<guid>`.
  - `GetAdmissionClassesHandler`: queries all admission classes with real-time seat counts.
  - `GetAdmissionClassByIdHandler`: queries single admission class by ID (supports `c_<guid>` and raw `<guid>`), returns 404 if not found.
- [x] Implement `ClassesController` (`POST /classes`, `GET /classes`, `GET /classes/{classId}`).
- [x] Add unit tests for ID helper, handlers, and controller (46/46 tests passing).

**Exit criteria**: API contract matches `docs/08-api-contract.md`, endpoints compile and test cleanly, seat counts calculate correctly.

---

## Phase 4 — Hold lifecycle and concurrency `[COMPLETED]`

- [x] Implement `ISeatHoldRepository` persistence port and EF Core PostgreSQL implementation:
  - Row-level locking on `admission_classes` using `SELECT ... FOR UPDATE` within a database transaction.
  - Single active hold per student policy: returns existing hold if requesting the same class; returns 409 Conflict if attempting to hold across different classes.
  - Automatic expiration of stale holds without requiring external background workers.
  - Idempotent confirmation: returns `200 OK` on repeated confirmations without double-counting seats.
  - Cancellation: marks active hold as `CANCELLED` and returns `204 No Content` (idempotent); rejects confirmation cancellation with `409 Conflict`.
- [x] Implement application use cases in `backend/src/SchoolAdmission.Application/SeatHolds/`:
  - `CreateSeatHoldHandler`: handles hold creation with configured duration (`HOLD_DURATION_SECONDS`, default 600).
  - `GetSeatHoldHandler`: validates student ownership (returns 404 for wrong owner/unknown hold).
  - `ConfirmSeatHoldHandler`: simulates payment, transitions status to `CONFIRMED`.
  - `CancelSeatHoldHandler`: cancels eligible holds.
  - `GetStudentSeatHoldsHandler`: returns current student's applications with class names and countdown timestamps.
- [x] Implement API controllers:
  - `HoldsController` (`POST /classes/{classId}/holds`, `GET /holds/{holdId}`, `POST /holds/{holdId}/confirm`, `DELETE /holds/{holdId}`).
  - `MeController` (`GET /me/holds`).
- [x] Add automated concurrency and lifecycle tests:
  - Tested 50 simultaneous competing requests for 5 seats: exactly 5 succeed, 45 return 409, 0 oversold.
  - Tested 25 simultaneous requests for the same student: exactly 1 creates hold, 24 return existing hold.
  - All 60 automated tests pass (`dotnet test backend/SchoolAdmission.sln`).

**Exit criteria**: Concurrency test passes, zero overselling, duplicate confirmations idempotent, all 60 tests pass.

---

## Phase 5 — Frontend foundation `[COMPLETED]`

- [x] Scaffold Vite + React 18 + TypeScript strict SPA in `frontend/`.
- [x] Configure Tailwind CSS adhering to `frontend-design` skill (editorial typography, custom academic color tokens, accessible focus rings, semantic components).
- [x] Configure React Router (`/`, `/my-applications`, `/admin`, `*` 404 fallback).
- [x] Implement typed API client (`frontend/src/lib/api-client.ts`) with automatic `X-Student-Id` header injection from localStorage and RFC 7807 `ProblemDetails` error parsing.
- [x] Implement TanStack Query client with window focus refetching and background polling.
- [x] Implement student identity entry bar (`StudentIdentityBar`) with validation, persistent storage, and edit/clear actions.
- [x] Implement admission class cards (`ClassCard`) displaying total, held, confirmed, and available seats with real-time capacity allocation bars and Apply actions.
- [x] Create multi-stage `frontend/Dockerfile` with Nginx on port 3000 and update `docker-compose.yml`.
- [x] Add automated unit tests for API client, student identity hook, and class card components (10/10 tests pass, `npm run build` succeeds in 3s).

**Exit criteria**: Frontend builds cleanly, passes all automated tests, and displays real API data.

---

## Phase 6 — Parent workflow `[COMPLETED]`

- [x] Implement `holds.api.ts` for fetching applications, simulated payment confirmation, and cancellation.
- [x] Implement `useMyHolds`, `useConfirmHold`, and `useCancelHold` hooks with automatic query cache invalidation and background polling (every 3.5s while active holds exist).
- [x] Implement `HoldCountdown.tsx` component computing exact minutes and seconds remaining, warning when under 2 minutes, and firing `onExpire` when time elapses.
- [x] Implement `ApplicationCard.tsx`:
  - Displays class name, hold ID, and live status badge.
  - `HELD`: shows countdown, simulated payment button ("Pay Admission Fee"), and "Cancel Application" button.
  - `CONFIRMED`: shows green badge and official confirmation notice.
  - `EXPIRED`: shows red badge, disables payment, and explains seat release.
  - `CANCELLED`: shows grey cancellation notice.
- [x] Implement complete `MyApplicationsPage.tsx`:
  - Restores applications on refresh or browser reopening by reusing `student_id` from `localStorage`.
  - Shows loading skeleton, empty states with CTA to browse classes, and success/error feedback alerts.
- [x] Add automated tests for countdown, application card actions, and hold API calls (21/21 frontend tests pass).

**Exit criteria**: Parent can select class, reserve seat, monitor live countdown, confirm admission with simulated payment, or cancel hold through UI.

---

## Developer Handoff: Deploying & Starting Phase 8

If you are a developer continuing from Phase 7 to Phase 8, follow these steps:

### 1. Prerequisites & Environment Setup
- .NET 10 SDK and Node.js (v20+) installed.
- Ensure `.env` exists:
  ```bash
  cp .env.example .env
  ```

### 2. Verify Current Backend & Frontend Builds
- Run backend tests (60 passed):
  ```bash
  dotnet test backend/SchoolAdmission.sln
  ```
- Run frontend tests (30 passed) & build:
  ```bash
  npm test --prefix frontend
  npm run build --prefix frontend
  ```

### 3. Run System Locally
- Start API:
  ```bash
  dotnet run --project backend/src/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
  ```
- Start Frontend (in separate terminal):
  ```bash
  npm run dev --prefix frontend
  ```
  Open browser at `http://localhost:3000`.

### 4. Phase 8 Next Steps (Quality and Submission)
Proceed with **Phase 8 — Quality and submission**:
1. Verify all acceptance criteria in `docs/21-acceptance-criteria.md`.
2. Verify full docker-compose orchestration (`docker-compose up --build`).
3. Final review of `ASSUMPTIONS.md` and `AI_USAGE.md`.
4. Ensure no uncommitted secrets or build artifacts.

---

## Phase 7 — Admin workflow `[COMPLETED]`

- Add `/admin` class creation form and class overview table.
- Add validation and success/error feedback.
- Aggregate capacity indicators and utilization progress bars.
- 9 unit tests verifying form validation, submission, error states, and metrics table.

Exit criteria: admin can create admission units through UI.

---

## Phase 8 — Quality and submission `[NEXT]`

---

## Phase 8 — Quality and submission

- Run all tests and builds.
- Verify clean Docker startup and persistence.
- Verify required API routes and headers.
- Complete `ASSUMPTIONS.md` and `AI_USAGE.md`.
- Add screenshots and concise setup instructions.
- Review public GitHub repository for secrets and accidental artifacts.

Exit criteria: all checks in `21-acceptance-criteria.md` pass.
