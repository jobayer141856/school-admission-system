# 05 — Clean Architecture

## Projects and responsibilities

### `SchoolAdmission.Domain`
- Entities/aggregate roots, value objects, enums, domain exceptions, and pure business rules.
- No dependency on ASP.NET Core, EF Core, PostgreSQL, or frontend code.

### `SchoolAdmission.Application`
- Use cases, command/query DTOs, validators, interfaces/ports, result types, and orchestration.
- Depends only on Domain and framework abstractions where needed.
- Defines repository/unit-of-work or persistence ports required by use cases.

### `SchoolAdmission.Infrastructure`
- EF Core DbContext and entity configurations.
- Repository implementations and transaction/concurrency implementation.
- PostgreSQL provider, migrations, system clock implementation if needed.
- Depends on Application and Domain.

### `SchoolAdmission.API`
- Controllers/minimal API endpoints, HTTP request/response contracts, middleware, dependency injection composition, Swagger, health checks.
- Depends on Application and Infrastructure for registration only.
- Does not contain domain business rules.

## Dependency direction

```text
API --------------------> Application
 |                            |
 +----> Infrastructure        v
             |              Domain
             +-------------->|
Application ----------------> Domain
```

Infrastructure implements interfaces defined inward. Domain never references outer projects.

## Suggested folders

```text
backend/src/
  SchoolAdmission.Domain/
    Common/
    AdmissionClasses/
    SeatHolds/
    Exceptions/
  SchoolAdmission.Application/
    Abstractions/
    AdmissionClasses/
      Commands/
      Queries/
      Dtos/
    SeatHolds/
      Commands/
      Queries/
      Dtos/
    Common/
    DependencyInjection.cs
  SchoolAdmission.Infrastructure/
    Persistence/
      Configurations/
      Migrations/
    Repositories/
    Services/
    DependencyInjection.cs
  SchoolAdmission.API/
    Controllers/
    Contracts/
    Middleware/
    Extensions/
    Program.cs
```

## Use-case naming

Prefer explicit use cases:
- `CreateAdmissionClass`
- `GetAdmissionClasses`
- `GetAdmissionClassById`
- `CreateSeatHold`
- `GetSeatHold`
- `ConfirmSeatHold`
- `CancelSeatHold`
- `GetStudentSeatHolds`

Avoid generic service classes with dozens of unrelated methods.

## CQRS guidance

Separate commands and queries by intent, but do not introduce MediatR or a message bus unless it provides real value. Plain application handlers are acceptable and easier to review.

## Transactions

The application use case owns transaction boundaries. The infrastructure persistence implementation provides atomic operations and row locking. Never split the capacity check and seat-hold insert into unrelated commits.
