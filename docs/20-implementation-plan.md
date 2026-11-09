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

## Developer Handoff: Deploying & Starting Phase 4

If you are a developer continuing from Phase 3 to Phase 4, follow these steps:

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
All 46 tests should pass without errors or warnings.

### 3. Run API Locally
To run the API on `http://localhost:8080`:
```bash
dotnet run --project backend/src/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```
- Probe health: `curl http://localhost:8080/health`
- Open Swagger UI: `http://localhost:8080/swagger`
- Test classes API:
  - Create class:
    ```bash
    curl -X POST http://localhost:8080/classes -H "Content-Type: application/json" -d "{\"name\":\"Class 6 - Morning Shift\",\"totalSeats\":120}"
    ```
  - List classes:
    ```bash
    curl http://localhost:8080/classes
    ```

### 4. Phase 4 Next Steps
Proceed with **Phase 4 — Hold lifecycle and concurrency**:
1. Implement Hold Abstractions & Repository:
   - `ISeatHoldRepository` in `backend/src/SchoolAdmission.Application/SeatHolds/`
   - Implementation in `backend/src/SchoolAdmission.Infrastructure/Repositories/SeatHoldRepository.cs`
   - Include row-level locking method (`SELECT ... FOR UPDATE` on `admission_classes`) to prevent race conditions during hold creation.
2. Implement Seat Hold Use-Cases:
   - `CreateSeatHoldHandler`:
     - Requires `X-Student-Id`.
     - Transaction begin.
     - Row-lock target class row (`SELECT ... FOR UPDATE`).
     - Expire stale holds for that class.
     - Check if student already has an active hold across the system (per `02-assumptions.md`: return existing hold if for same unit, or 409 if for another unit).
     - Calculate remaining capacity (`totalSeats - activeHeld - confirmed`).
     - If available <= 0, return 409 Conflict.
     - Insert `SeatHold` with expiry `now + HOLD_DURATION_SECONDS` (default 600).
     - Commit transaction.
   - `GetSeatHoldHandler`:
     - Requires `X-Student-Id`.
     - Validates ownership (returns 404 for wrong owner/unknown hold).
     - Evaluates expiry before returning.
   - `ConfirmSeatHoldHandler`:
     - Requires `X-Student-Id`.
     - Validates ownership.
     - Idempotent: returns 200 `CONFIRMED` if already confirmed without re-incrementing count.
     - Transitions `HELD` to `CONFIRMED` and sets `confirmed_at_utc`. Returns 409 if expired/cancelled.
   - `CancelSeatHoldHandler`:
     - Requires `X-Student-Id`.
     - Validates ownership.
     - Returns 204 No Content for cancelled active hold (idempotent if already cancelled).
     - Returns 409 if already confirmed or expired.
   - `GetStudentSeatHoldsHandler`:
     - Endpoint `GET /me/holds`.
     - Returns current student's holds with class names and remaining expiry.
3. Implement `HoldsController` and `MeController` (or `/me/holds` endpoint).
4. Add automated concurrency tests proving capacity is never oversold.

---

## Phase 4 — Hold lifecycle and concurrency `[NEXT]`

- Implement create hold with transaction and locking (`SELECT ... FOR UPDATE`).
- Implement get hold and student hold list (`GET /holds/{holdId}`, `GET /me/holds`).
- Implement confirm and cancel (`POST /holds/{holdId}/confirm`, `DELETE /holds/{holdId}`).
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
