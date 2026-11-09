# 23 — AI Usage Log

Complete this file honestly before submission. Do not claim tools or checks that were not used.

## AI tools used

| Tool | Purpose | Files/areas | Human verification |
|---|---|---|---|
| Google Antigravity (Gemini 3.8 Flash) | Clean Architecture, domain entities, persistence, hold lifecycle, and complete parent UI workflow | `backend/`, `frontend/`, `docker-compose.yml`, `.github/`, `docs/20-implementation-plan.md` | Compiled with `dotnet build` and `npm run build`, tested with `dotnet test` and `npm test` |

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
    - Unit tests for ID parsing, use cases, and controller actions.
  - Phase 4 Hold lifecycle and concurrency:
    - DTOs: `SeatHoldResponse`, `SeatHoldDetailResponse`, `ConfirmHoldResponse`, `StudentHoldItemResponse`.
    - `ISeatHoldRepository` and EF Core `SeatHoldRepository` with `SELECT ... FOR UPDATE` row locking.
    - Use cases: `CreateSeatHoldHandler`, `GetSeatHoldHandler`, `ConfirmSeatHoldHandler`, `CancelSeatHoldHandler`, `GetStudentSeatHoldsHandler`.
    - Controllers: `HoldsController` and `MeController`.
    - Automated concurrency stress tests proving capacity is never oversold.
  - Phase 5 Frontend foundation:
    - Vite React 18 TypeScript application in `frontend/`.
    - Tailwind CSS configuration following `frontend-design` skill.
    - Router, Query client, and typed `apiClient` with automatic `X-Student-Id` header injection.
    - Components: `Navbar`, `Footer`, `Layout`, `StudentIdentityBar`, `ClassCard`, `ClassList`, and `AdmissionsPage`.
    - Multi-stage `frontend/Dockerfile` with Nginx and updated `docker-compose.yml`.
  - Phase 6 Parent workflow:
    - `holds.api.ts` with typed methods for `getMyHolds`, `confirmHold`, and `cancelHold`.
    - Hooks `useMyHolds`, `useConfirmHold`, and `useCancelHold` with dual-query invalidation and fast polling on active holds.
    - `HoldCountdown.tsx` component with live seconds timer, 2-minute urgency warning, and `onExpire` hook.
    - `ApplicationCard.tsx` with status handling for `HELD`, `CONFIRMED`, `EXPIRED`, and `CANCELLED`.
    - Simulated payment button calling confirmation endpoint, and cancellation button.
    - Completed `MyApplicationsPage.tsx` supporting refresh recovery from `localStorage`.
    - Unit tests for countdown, application card, and API client (21 frontend tests).
- Which prompts or approaches were materially useful:
  - User requested targeting `.NET 10` (`net10.0`).
  - Frontend visual design direction per `frontend-design` skill.
  - Phased progression with explicit developer handoff documentation.
- Any incorrect suggestions or hallucinations found:
  - `WriteAsJsonAsync` initially defaulted content type to `application/json; charset=utf-8` instead of `application/problem+json`; corrected.
  - Test files initially caused `tsc` type resolution issues during `npm run build`; resolved by adding test exclusions to `tsconfig.json`.
- What was changed after review:
  - Updated middleware to explicitly pass `contentType: "application/problem+json"` to `WriteAsJsonAsync`.
  - Configured embedded Swagger UI along with Scalar API reference for `/swagger` route compatibility.

## Verification performed

| Check | Command/action | Result |
|---|---|---|
| Backend tests | `dotnet test backend/SchoolAdmission.sln` | Pass (60 tests passed) - 2026-11-09 |
| Backend build | `dotnet build backend/SchoolAdmission.sln` | Pass (0 errors, 0 warnings) - 2026-11-09 |
| Concurrency test | `SimultaneousHoldRequests_NeverExceedCapacity_AndNeverOversell` (50 concurrent requests competing for 5 seats) | Pass (0 oversold, exactly 5 created, 45 full) - 2026-11-09 |
| Same-student concurrency test | `SimultaneousRequests_ForSameStudent_NeverCreateDuplicateActiveHolds` (25 concurrent requests for same student) | Pass (exactly 1 created, 24 returned existing hold) - 2026-11-09 |
| Frontend tests | `npm test --prefix frontend` | Pass (21 tests passed) - 2026-11-09 |
| Frontend build | `npm run build --prefix frontend` | Pass (built in 2.69s, 0 errors) - 2026-11-09 |
| EF Core migration | `dotnet ef migrations add InitialCreate` | Pass (migration created cleanly) - 2026-11-09 |
| Docker Compose config validation | `docker-compose config` | Pass (valid configuration with postgres, api, frontend) - 2026-11-09 |
| Health check probe | `curl http://localhost:8080/health` | Pass (503 Service Unavailable when DB is offline, verified functional) - 2026-11-09 |
| Swagger endpoint probe | `curl http://localhost:8080/swagger` | Pass (HTTP 200 OK) - 2026-11-09 |
| Docker startup | `docker compose up --build` | Pending Docker Desktop daemon start |

## Reflection

Explain one AI suggestion you rejected or corrected and why. Be specific and truthful.
- Rejected injecting `Microsoft.Extensions.Configuration.IConfiguration` directly into `CreateSeatHoldHandler` in `SchoolAdmission.Application` to preserve Clean Architecture project boundaries. Instead, introduced `HoldOptions` to decouple application use-case logic from framework configuration libraries.
