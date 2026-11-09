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
  - `HoldsController`:
    - `POST /classes/{classId}/holds` (requires `X-Student-Id`, returns 201 Created or 409 Conflict).
    - `GET /holds/{holdId}` (requires `X-Student-Id`, validates ownership).
    - `POST /holds/{holdId}/confirm` (requires `X-Student-Id`, idempotent 200 OK).
    - `DELETE /holds/{holdId}` (requires `X-Student-Id`, returns 204 No Content).
  - `MeController`:
    - `GET /me/holds` (requires `X-Student-Id`, lists student's holds).
- [x] Add automated concurrency and lifecycle tests:
  - Tested 50 simultaneous competing requests for 5 seats: exactly 5 succeed, 45 return 409, 0 oversold.
  - Tested 25 simultaneous requests for the same student: exactly 1 creates hold, 24 return existing hold.
  - All 60 automated tests pass (`dotnet test backend/SchoolAdmission.sln`).

**Exit criteria**: Concurrency test passes, zero overselling, duplicate confirmations idempotent, all 60 tests pass.

---

## Developer Handoff: Deploying & Starting Phase 5

If you are a developer continuing from Phase 4 to Phase 5, follow these steps:

### 1. Prerequisites & Environment Setup
- .NET 10 SDK installed (or Docker Compose).
- Node.js (v20+) installed.
- Clone the repository and navigate to root:
  ```bash
  cp .env.example .env
  ```

### 2. Verify Current Backend Build and Tests
Run all unit and concurrency tests across the backend:
```bash
dotnet test backend/SchoolAdmission.sln
```
All 60 tests should pass without errors or warnings.

### 3. Run API Locally
To run the API on `http://localhost:8080`:
```bash
dotnet run --project backend/src/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```
- Probe health: `curl http://localhost:8080/health`
- Open Swagger UI: `http://localhost:8080/swagger`
- Test hold creation:
  ```bash
  curl -X POST http://localhost:8080/classes/{classId}/holds -H "X-Student-Id: 20152691234567890"
  ```
- Test student holds:
  ```bash
  curl http://localhost:8080/me/holds -H "X-Student-Id: 20152691234567890"
  ```

### 4. Phase 5 Next Steps (Frontend Foundation)
Follow the `frontend-design` skill guidelines:
1. Initialize Vite + React + TypeScript in `frontend/`:
   ```bash
   npm create vite@latest frontend -- --template react-ts
   ```
2. Install UI dependencies:
   - React Router (`react-router-dom`)
   - TanStack Query (`@tanstack/react-query`)
   - Axios or typed fetch client
   - Lucide React icons
   - Tailwind CSS for styling
3. Design Direction (`frontend-design` skill):
   - Palette: Deep Slate `#0f172a`, Off-white Surface `#ffffff`, Warm Canvas `#f8fafc`, Rich Indigo Accent `#4338ca`, Muted Slate `#64748b`, Alert Green `#059669`, Alert Red `#e11d48`.
   - Typography: Clean modern sans-serif with distinct weights.
   - Layout: Non-generic responsive layout. Open with clear student birth-registration number entry, followed by real-time admission class cards.
4. Core modules to build:
   - `frontend/src/lib/api-client.ts`: Typed client attaching `X-Student-Id` header from local storage.
   - `frontend/src/features/admission-classes/`: Hooks and components to fetch `GET /classes` and render seat counts.
   - `frontend/src/features/identity/`: Component to input and save the student birth-registration number.
5. Create `frontend/Dockerfile` with Nginx multi-stage build.

---

## Phase 5 — Frontend foundation `[NEXT]`

- Create Vite React TypeScript app in `frontend/`.
- Configure router, API client, query client, and styling (using `frontend-design` skill).
- Add parent identity entry (`X-Student-Id`) and admission class cards with real-time seat counts.

Exit criteria: frontend displays real API data in Docker.

---

## Phase 6 — Parent workflow

- Add create hold, my applications, countdown, simulated payment, cancellation.
- Add refresh recovery and polling.
- Add loading/error/empty/conflict states.

Exit criteria: parent completes entire workflow through UI.

---

## Phase 7 — Admin workflow

- Add `/admin` class creation form and class overview.
- Add validation and success/error feedback.

Exit criteria: admin can create admission units through UI.

---

## Phase 8 — Quality and submission

- Run all tests and builds.
- Verify clean Docker startup and persistence.
- Verify required API routes and headers.
- Complete `ASSUMPTIONS.md` and `AI_USAGE.md`.
- Add screenshots and concise setup instructions.
- Review public GitHub repository for secrets and accidental artifacts.

Exit criteria: all checks in `21-acceptance-criteria.md` pass.
