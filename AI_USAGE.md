# 23 — AI Usage Log

Complete this file honestly before submission. Do not claim tools or checks that were not used.

## AI tools used

| Tool | Purpose | Files/areas | Human verification |
|---|---|---|---|
| Google Antigravity (Gemini 3.8 Flash) | Clean Architecture, domain entities, persistence, and Phase 3 API handlers | `backend/`, `docker-compose.yml`, `.github/`, `docs/20-implementation-plan.md` | Compiled with `dotnet build`, tested with `dotnet test`, tested controllers with unit tests |

## AI-generated or AI-assisted work

Describe:
- Which code or documentation was generated or refactored with AI:
  - Phase 0 repository configuration: `.gitignore`, `.dockerignore`, `.env.example`, `docker-compose.yml`, and CI pipeline `.github/workflows/ci.yml`.
  - Phase 1 Clean Architecture layers targeting .NET 10: `SchoolAdmission.Domain`, `SchoolAdmission.Application`, `SchoolAdmission.Infrastructure`, `SchoolAdmission.API`, and `SchoolAdmission.UnitTests`.
  - Phase 2 Domain and persistence:
    - Domain aggregates: `AdmissionClass`, `SeatHold`, value object `StudentId`, and enum `SeatHoldStatus`.
    - EF Core Fluent API configurations: `AdmissionClassConfiguration` and `SeatHoldConfiguration`.
    - Generated EF Core migration: `InitialCreate`.
    - Domain unit tests with comprehensive validation and state transition coverage.
  - Phase 3 Read/create admission classes:
    - DTOs and `IdHelper` for formatting IDs (`c_<guid>`).
    - `IAdmissionClassRepository` abstraction in Application and EF Core implementation in Infrastructure.
    - Application handlers: `CreateAdmissionClassHandler`, `GetAdmissionClassesHandler`, `GetAdmissionClassByIdHandler`.
    - `ClassesController` endpoints: `POST /classes`, `GET /classes`, `GET /classes/{classId}`.
    - Unit tests for ID parsing, use cases, and controller actions (46 tests total).
- Which prompts or approaches were materially useful:
  - User requested targeting `.NET 10` (`net10.0`).
  - Phased progression with explicit developer handoff documentation.
- Any incorrect suggestions or hallucinations found:
  - `WriteAsJsonAsync` initially defaulted content type to `application/json; charset=utf-8` instead of `application/problem+json`; caught during unit test execution and corrected.
  - Package version mismatch warning in UnitTests resolved.
- What was changed after review:
  - Updated middleware to explicitly pass `contentType: "application/problem+json"` to `WriteAsJsonAsync`.
  - Configured embedded Swagger UI along with Scalar API reference for `/swagger` route compatibility.

## Verification performed

| Check | Command/action | Result |
|---|---|---|
| Backend tests | `dotnet test backend/SchoolAdmission.sln` | Pass (46 tests passed) - 2026-11-09 |
| Backend build | `dotnet build backend/SchoolAdmission.sln` | Pass (0 errors, 0 warnings) - 2026-11-09 |
| EF Core migration | `dotnet ef migrations add InitialCreate` | Pass (migration created cleanly) - 2026-11-09 |
| Docker Compose config validation | `docker-compose config` | Pass (valid configuration) - 2026-11-09 |
| Health check probe | `curl http://localhost:8080/health` | Pass (503 Service Unavailable when DB is offline, verified functional) - 2026-11-09 |
| Swagger endpoint probe | `curl http://localhost:8080/swagger` | Pass (HTTP 200 OK) - 2026-11-09 |
| Frontend tests | `npm run test` | Pending Phase 5 |
| Frontend build | `npm run build` | Pending Phase 5 |
| Docker startup | `docker compose up --build` | Pending Docker Desktop daemon start |
| Concurrency test | [test name] | Pending Phase 4 |

## Reflection

Explain one AI suggestion you rejected or corrected and why. Be specific and truthful.
- Rejected placing `DatabaseHealthCheck` in `SchoolAdmission.Infrastructure` to prevent polluting the Infrastructure project with ASP.NET Core hosting framework references (`Microsoft.AspNetCore.App`), keeping Infrastructure purely dependent on Entity Framework Core and Application abstractions per Clean Architecture.
