# 13 — UI/UX Specification

The UI should be clean, responsive, accessible, and understandable. Visual polish is useful, but correctness and clarity are more important.

## Parent page `/`

1. Header: School Admission System.
2. Student identity field labelled “Student birth-registration number”.
3. Save/continue action.
4. Admission class cards/table:
   - class/shift name
   - total seats
   - held seats
   - confirmed seats
   - available seats
   - Apply button
5. If no student ID is entered, explain that it is required before applying.
6. If no seats remain, disable Apply and show “No seats available”.

## My applications `/my-applications`

- Show class name, hold ID, status, created/expiry time.
- For active `HELD` applications show remaining time and “Pay admission fee”.
- “Pay admission fee” calls the confirm endpoint; it simulates successful payment.
- Show “Cancel application” for eligible active holds.
- For `EXPIRED`, show an expired label and disable payment.
- For `CONFIRMED`, show admission confirmed.
- Empty state: “No applications found for this student”.

## Admin page `/admin`

- Form fields: admission class name, total seats.
- Submit creates a class through `POST /classes`.
- List all classes and current counts.
- Validate positive integer seat capacity and non-empty name.
- Clearly label this as a demo admin interface without authentication.

## Feedback states

- Loading skeleton/spinner.
- Empty list.
- Field-level validation.
- No available seat (409).
- Active hold already exists (409).
- Expired hold.
- Server unavailable/network error.
- Success toast/inline confirmation.
- Never display raw exception messages or blank pages.

## Accessibility

- Semantic labels and buttons.
- Keyboard navigable forms and dialogs.
- Visible focus indicators.
- Adequate contrast.
- Do not communicate status by color alone.
- Use polite live regions for asynchronous feedback.
