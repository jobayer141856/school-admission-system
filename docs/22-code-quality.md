# 22 — Code Quality

## .NET

- Enable nullable reference types.
- Use analyzers and consistent formatting.
- Use `async`/`await` for I/O and pass cancellation tokens.
- Use `DateTimeOffset`/UTC and injected `TimeProvider`.
- Keep controllers/endpoints thin.
- Keep EF Core configuration in separate configuration classes.
- Avoid exposing EF entities as API response contracts.
- Use structured logs and centralized exception handling.
- Avoid static mutable state.
- Keep dependency direction enforceable through project references/tests.

## TypeScript

- Enable `strict` mode.
- Avoid `any`; define API request/response types.
- Centralize API client and error mapping.
- Keep feature code grouped by business capability.
- Use form schemas and server-side validation.
- Avoid duplicating server state in component state.

## General

- Prefer readable code over clever code.
- Keep functions cohesive.
- Add comments for non-obvious concurrency/locking behavior.
- Do not add dependencies without a clear need.
- Update docs and tests with behavior changes.
