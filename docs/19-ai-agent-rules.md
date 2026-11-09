# 19 — AI Coding Agent Rules

You are implementing the School Admission System in this monorepo. Treat this file and the other project documents as the source of truth.

## Before coding

1. Read `README.md`.
2. Read all relevant files under `docs/`.
3. Inspect the existing repository before changing it.
4. Do not overwrite working code or configuration without understanding it.
5. Identify the current implementation phase from `20-implementation-plan.md`.

## Architecture rules

- Backend must use DDD + Clean Architecture.
- Keep Domain independent of EF Core, ASP.NET Core, PostgreSQL, and UI.
- Keep use-case orchestration in Application.
- Keep persistence and external implementations in Infrastructure.
- Keep HTTP concerns in API.
- Frontend must call the API; never access the database directly.
- Keep exact assignment endpoints and JSON contracts.
- Do not add unnecessary microservices, message brokers, CQRS frameworks, or abstractions.

## Business correctness

- Never implement capacity as an unprotected read-count-then-insert.
- Use PostgreSQL transaction and row-lock strategy from `10-concurrency-strategy.md`.
- Every capacity-changing code path must use the same lock protocol.
- Use an injected clock.
- Ensure expiry is handled even without a background worker.
- Make confirmation idempotent.
- Enforce student ownership.
- Add tests for every business rule and failure path.

## Docker rules

- `docker compose up --build` must start the whole system.
- Keep root `docker-compose.yml`.
- Persist PostgreSQL data in a named volume.
- Ensure browser API URL is host-reachable.
- Do not require local .NET/Node/PostgreSQL installation.
- Do not commit `.env`, secrets, or local build artifacts.

## Coding rules

- Use nullable reference types and strict TypeScript.
- Prefer clear names and small cohesive methods.
- Validate at system boundaries.
- Use async APIs and cancellation tokens in .NET.
- Use structured logging.
- Do not swallow exceptions.
- Do not return raw exception messages.
- Avoid `TODO` placeholders in completed work.
- Do not claim tests pass unless you ran them and observed success.

## Change workflow

For each feature:
1. State the files/modules to be changed.
2. Implement the smallest coherent vertical slice.
3. Add/update tests.
4. Run relevant tests and builds.
5. Review API contract and architecture boundaries.
6. Update documentation if behavior or assumptions change.
7. Summarize changed files, commands run, and actual results.

## Git workflow

Use small, descriptive commits. Do not create one giant final commit. Never rewrite history or force-push unless explicitly requested.
