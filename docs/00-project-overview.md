# 00 — Project Overview

## Goal

Build a school admission system for a new academic year. Parents can view admission units, identify a student using a birth-registration number, reserve a seat temporarily, and simulate payment to confirm admission. School administrators can create admission units and configure capacity.

## Actors

1. **Parent/guardian** — enters the student's birth-registration number, views available seats, creates a hold, checks its status, confirms it through simulated payment, or cancels it.
2. **School administrator** — creates and views class-and-shift admission units and their seat counts.

## Core workflow

1. Parent enters a student birth-registration number.
2. Parent selects a class-and-shift admission unit.
3. API atomically reserves one seat and returns a hold ID and expiry timestamp.
4. Parent confirms the simulated payment before expiry.
5. If the hold expires or is cancelled, the seat becomes available again.
6. Parent can reload the page or reopen the browser and still see their applications using the same student ID.

## Non-goals

- Real payment gateway integration.
- Lottery or merit-based allocation.
- Identity verification or OTP.
- Multi-school tenancy.
- Document upload, admission exam, or approval workflow.
- Production-grade admin identity management unless explicitly added as an extension.

## Success criteria

The exact API contract, business rules, persistence, concurrency guarantees, Docker startup, UI states, and tests in this documentation are the definition of done.
