# 15 — Error Handling

## HTTP mapping

| Condition | Status |
|---|---:|
| Invalid body/header | 400 |
| Missing class/hold | 404 |
| No seats available | 409 |
| Student already has another active hold | 409 |
| Hold expired before confirmation | 409 |
| Hold already confirmed | 200 idempotent success |
| Unsupported state transition | 409 |
| Unexpected server failure | 500 |

## Backend

- Use centralized exception-to-ProblemDetails middleware.
- Use domain/application exceptions for expected business failures.
- Log unexpected exceptions with correlation/trace ID.
- Do not return internal exception text to clients.
- Ensure validation failures never become 500.

## Frontend

Map API errors to human-friendly messages:
- 400: “Please check the information you entered.”
- 404: “This application could not be found.”
- 409 no capacity: “Sorry, this class is now full.”
- 409 expired: “Your reservation has expired. Please apply again.”
- Network/server: “We could not connect. Please try again.”

Keep field validation close to the relevant input. Never rely only on console logging.
