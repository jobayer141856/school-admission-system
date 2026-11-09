# 08 — API Contract

The following routes and response semantics must remain compatible with the assignment. Do not rename or wrap these routes in a version prefix.

Base URL: `http://localhost:8080`

All responses are JSON except successful cancellation (`204 No Content`).

## 1. Create admission class

`POST /classes`

Request:
```json
{ "name": "Class 6 - Morning Shift", "totalSeats": 120 }
```

Success `201`:
```json
{ "id": "c_123", "name": "Class 6 - Morning Shift", "totalSeats": 120 }
```

Validation errors: `400`. Duplicate name policy may be enforced as `409` if documented.

## 2. List classes and seat counts

`GET /classes`

Success `200`:
```json
[
  {
    "id": "c_123",
    "name": "Class 6 - Morning Shift",
    "totalSeats": 120,
    "heldSeats": 15,
    "confirmedSeats": 80,
    "availableSeats": 25
  }
]
```

## 3. Get class

`GET /classes/{classId}`

Success `200`: same shape as one item from `GET /classes`.

Unknown ID: `404`.

## 4. Create seat hold

`POST /classes/{classId}/holds`

Required header: `X-Student-Id: <birth-registration-number>`

Success `201`:
```json
{
  "holdId": "h_789",
  "classId": "c_123",
  "studentId": "20152691234567890",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}
```

No capacity, or student already has a different active hold: `409`.

Unknown class: `404`. Missing/invalid student header: `400`.

## 5. Get hold

`GET /holds/{holdId}`

Success `200`:
```json
{
  "holdId": "h_789",
  "classId": "c_123",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}
```

Status values: `HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`.

Require `X-Student-Id` and verify ownership. Unknown hold: `404`; wrong owner: `404` (avoid disclosing another student's hold).

## 6. Confirm hold

`POST /holds/{holdId}/confirm`

Required header: `X-Student-Id`.

Success `200`:
```json
{ "holdId": "h_789", "status": "CONFIRMED" }
```

- Simulated payment only; no payment gateway.
- Expired/cancelled hold: `409`.
- Already confirmed hold: return the same successful result without incrementing confirmed seats again.
- Wrong owner: `404`.

## 7. Cancel hold

`DELETE /holds/{holdId}`

Required header: `X-Student-Id`.

Success: `204 No Content`.

- Cancel a `HELD` hold.
- Already cancelled may return `204` (idempotent).
- Confirmed/expired hold: `409` or `204` only if the implementation documents a consistent policy; recommended policy is `409` for terminal non-cancellable states.
- Wrong owner/unknown hold: `404`.

## 8. List current student's holds

`GET /me/holds`

Required header: `X-Student-Id`.

Success `200`:
```json
[
  {
    "holdId": "h_789",
    "classId": "c_123",
    "className": "Class 6 - Morning Shift",
    "status": "HELD",
    "expiresAt": "2026-11-01T04:10:00Z"
  }
]
```

## Additional endpoints

- `GET /health` — health probe.
- Swagger/OpenAPI in Development.

## Error response

Use RFC 7807 / ASP.NET `ProblemDetails` consistently:
```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Seat is no longer available",
  "status": 409,
  "detail": "No available seats remain for this admission class."
}
```

Never return stack traces or database details to the browser.
