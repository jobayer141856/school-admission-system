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

## Developer Handoff: Deploying & Starting Phase 3

If you are a developer continuing from Phase 2 to Phase 3, follow these steps:

### 1. Prerequisites & Environment Setup
- .NET 10 SDK installed (or Docker Compose).
- Clone the repository and navigate to root:
  ```bash
  cp .env.example .env
  ```

### 2. Verify Current Build and Tests
Run all unit tests across the solution:
```bash
dotnet test backend/SchoolAdmission.sln
```
All 35 tests should pass without errors or warnings.

### 3. Run API Locally
To run the API on `http://localhost:8080`:
```bash
dotnet run --project backend/src/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```
- Probe health: `curl http://localhost:8080/health`
- Open Swagger UI: `http://localhost:8080/swagger`
- Open Scalar API Docs: `http://localhost:8080/scalar/v1`

### 4. Phase 3 Next Steps
Proceed with **Phase 3 — Read/create admission classes**:
1. Application layer use-cases in `backend/src/SchoolAdmission.Application/AdmissionClasses/`:
   - `CreateAdmissionClassCommand` & Handler: validates name and positive capacity, saves to `AdmissionClasses` table, returns DTO with prefixed ID `c_<guid>`.
   - `GetAdmissionClassesQuery` & Handler: queries all admission classes and dynamically calculates:
     - `heldSeats`: count where `status = 'HELD' AND expires_at_utc > now`
     - `confirmedSeats`: count where `status = 'CONFIRMED'`
     - `availableSeats`: `totalSeats - heldSeats - confirmedSeats`
   - `GetAdmissionClassByIdQuery` & Handler: queries single class with same seat calculations, returns 404 if not found.
2. API Controller:
   - `ClassesController` in `backend/src/SchoolAdmission.API/Controllers/ClassesController.cs`:
     - `POST /classes` -> `201 Created` with `Location` header.
     - `GET /classes` -> `200 OK` list.
     - `GET /classes/{classId}` -> `200 OK` or `404 Not Found`.
3. Add integration tests verifying exact API contracts from `docs/08-api-contract.md`.

---

## Phase 3 — Read/create admission classes `[NEXT]`

- Implement `POST /classes`.
- Implement `GET /classes`.
- Implement `GET /classes/{classId}``.
- Calculate counts from persisted records (`heldSeats`, `confirmedSeats`, `availableSeats`).

Exit criteria: API contract integration tests pass.

---

## Phase 4 — Hold lifecycle and concurrency

- Implement create hold with transaction and locking.
- Implement get hold and student hold list.
- Implement confirm and cancel.
- Implement expiry processing.
- Add PostgreSQL concurrency tests.

Exit criteria: overselling test passes; duplicate confirmation is idempotent.

---

## Phase 5 — Frontend foundation

- Create Vite React TypeScript app.
- Configure router, API client, query client, styling (using `frontend-design` skill).
- Add parent identity entry and class list.

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
