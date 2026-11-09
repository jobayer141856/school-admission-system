# 14 — Security

## Assignment scope

The assignment uses `X-Student-Id` as an identity key and does not specify authentication. This is not secure proof of identity. Document this limitation prominently; do not claim production readiness.

## Required safeguards

- Validate and normalize all input.
- Enforce ownership checks on all student-specific endpoints.
- Do not reveal whether another student's hold exists; return `404` for unauthorized hold access.
- Use parameterized SQL/EF Core parameters.
- Never expose connection strings, stack traces, or secrets.
- Configure CORS only for expected frontend origins.
- Keep database port internal to Compose by default; do not publish `5432` unless local debugging explicitly requires it.
- Use non-root container users where practical.
- Use environment variables for secrets and configuration.
- Commit `.env.example`, never `.env`.
- Set request size and reasonable validation limits.
- Use HTTPS in production behind a reverse proxy.
- Apply rate limiting to hold creation and admin endpoints if deploying publicly.

## Production extensions

Before real-world use, add verified parent authentication, admin authorization, audit logging, rate limiting, secure student identity verification, and real payment-provider integration with webhook signature validation and idempotency.
