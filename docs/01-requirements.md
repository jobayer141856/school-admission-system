# 01 — Requirements

## Functional requirements

### Parent experience

- No login screen is required by the assignment.
- A parent enters a student's birth-registration number.
- The UI uses that value as the student's identity for API requests via `X-Student-Id`.
- Display all class-and-shift admission units.
- Display total, held, confirmed, and available seats.
- Allow a parent to create a seat hold if capacity exists.
- Display hold ID, status, and expiry time.
- Allow the parent to view all their holds.
- Allow a parent to confirm a valid hold using the simulated payment action.
- Allow a parent to cancel an eligible hold.
- Restore the parent's current applications after refresh or browser restart by reusing the student ID.

### School administrator experience

- Provide an admin screen to create a class-and-shift admission unit.
- Each admission unit has a name and total seat capacity.
- Show the list of admission units and live seat counts.
- Validate input and show readable success/error messages.

### Backend

- Implement all endpoints in `08-api-contract.md`.
- Enforce student ownership for hold operations.
- Enforce all invariants in `09-business-rules.md`.
- Persist data in PostgreSQL.
- Use environment variables for database connection and hold duration.
- Return 4xx for invalid/user-caused conditions; do not leak unhandled exceptions as 500 responses.

## Non-functional requirements

- Root `docker-compose.yml` starts database, API, and frontend.
- No local runtime/database installations are required beyond Docker.
- Data survives container recreation and `docker compose down` / `up`.
- API listens on port 8080 inside and outside the Compose environment.
- Frontend API URL is configurable through an environment variable at build/runtime.
- Support simultaneous hold requests without overselling.
- Automated tests include a concurrency test proving capacity is never exceeded.
- UI communicates loading, empty, validation, conflict, expiry, and server-error states.
- Page refresh must not be required to see status/seat-count changes.

## Acceptance mapping

See `21-acceptance-criteria.md` for testable acceptance checks.
