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
- [x] Add unit tests for Clean Architecture boundaries, `IClock`, and ProblemDetails status code mapping (7/7 tests passing).

**Exit criteria**: Backend builds cleanly, all automated tests pass, and API starts on port 8080 with `/health` and `/swagger` responsive.

---

## Developer Handoff: Deploying & Starting Phase 2

If you are a developer continuing from Phase 1 to Phase 2, follow these steps:

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
All tests should pass without errors or warnings.

### 3. Run API Locally
To run the API on `http://localhost:8080`:
```bash
dotnet run --project backend/src/SchoolAdmission.API/SchoolAdmission.API.csproj --urls http://localhost:8080
```
- Probe health: `curl http://localhost:8080/health`
- Open Swagger UI: `http://localhost:8080/swagger`
- Open Scalar API Docs: `http://localhost:8080/scalar/v1`

### 4. Run with Docker Compose
Start PostgreSQL and API together:
```bash
docker compose up --build
```

### 5. Phase 2 Next Steps
Proceed with **Phase 2 — Domain and persistence**:
1. Implement `AdmissionClass` aggregate in `backend/src/SchoolAdmission.Domain/AdmissionClasses/`:
   - Properties: `Id`, `Name`, `TotalSeats`, `CreatedAtUtc`.
   - Invariants: Non-empty name, bounded length, positive capacity (`TotalSeats > 0`).
2. Implement `SeatHold` aggregate in `backend/src/SchoolAdmission.Domain/SeatHolds/`:
   - Properties: `Id`, `AdmissionClassId`, `StudentId`, `Status`, `CreatedAtUtc`, `ExpiresAtUtc`, `ConfirmedAtUtc`, `CancelledAtUtc`.
   - Enums: `SeatHoldStatus` (`HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`).
   - Behaviors: `Create(...)`, `Confirm(now)`, `Expire(now)`, `Cancel(now)`, `IsExpired(now)`.
3. Add EF Core entity configurations in `backend/src/SchoolAdmission.Infrastructure/Persistence/Configurations/`:
   - Table `admission_classes` and table `seat_holds` matching `docs/07-database-design.md`.
   - Indexes and check constraints (`total_seats > 0`).
4. Generate initial migration:
   ```bash
   dotnet ef migrations add InitialCreate --project backend/src/SchoolAdmission.Infrastructure --startup-project backend/src/SchoolAdmission.API
   ```
5. Add domain unit tests with fake clock (`TimeProvider`) in `backend/tests/SchoolAdmission.UnitTests`.

---

## Phase 2 — Domain and persistence `[NEXT]`

- Implement AdmissionClass and SeatHold aggregates.
- Implement status transitions and validation.
- Configure tables, indexes, and migrations.
- Add fake-clock domain tests.

Exit criteria: domain tests pass and schema is created.

---

## Phase 3 — Read/create admission classes

- Implement `POST /classes`.
- Implement `GET /classes`.
- Implement `GET /classes/{classId}`.
- Calculate counts from persisted records.

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
