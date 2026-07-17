# Healthcare Inventory

Monorepo with two ASP.NET Core 8 Web APIs, both using Clean Architecture, CQRS (MediatR), FluentValidation, AutoMapper, Serilog, and SQL Server:

| API | Solution | Purpose |
|-----|----------|---------|
| **Healthcare Appointment & Availability** | `HealthcareScheduling.sln` | Doctor scheduling, JWT auth, overlap prevention |
| **Order & Inventory** | `OrderInventory.sln` | Order booking, stock reservation, idempotent mock payments |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Docker example below)

## Database setup (shared)

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

Each API uses its own database / connection string under `appsettings.Development.json`.

---

# 1. Healthcare Appointment & Availability API

Production-ready API for doctor appointment scheduling with strict overlap prevention and concurrency control, plus JWT authentication and role-based access.

## Solution structure

```
src/
  HealthcareScheduling.Api/           # Controllers, middleware, Serilog, Swagger
  HealthcareScheduling.Application/   # CQRS handlers, validators, DTOs, AutoMapper
  HealthcareScheduling.Domain/        # Entities, enums, repository interfaces
  HealthcareScheduling.Persistence/   # EF Core, repositories, migrations
  HealthcareScheduling.Infrastructure/# JWT, password hashing, DateTimeProvider
tests/
  HealthcareScheduling.UnitTests/
  HealthcareScheduling.IntegrationTests/
```

## Run migrations

Migrations are applied automatically on startup via `DbSeeder`. To create or update migrations manually:

```bash
dotnet ef migrations add InitialCreate --project src/HealthcareScheduling.Persistence --startup-project src/HealthcareScheduling.Api
dotnet ef database update --project src/HealthcareScheduling.Persistence --startup-project src/HealthcareScheduling.Api
```

Connection string: `src/HealthcareScheduling.Api/appsettings.Development.json`

## Run the API

```bash
dotnet run --project src/HealthcareScheduling.Api
```

Swagger UI: `http://localhost:5081/swagger`

### Seeded credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@healthcare.local` | `Password123!` | Admin |
| `user@healthcare.local` | `Password123!` | User (patient) |
| `doctor@healthcare.local` | `Password123!` | Doctor |

Seed IDs:
- Doctor ID: `11111111-1111-1111-1111-111111111111`
- Patient / User ID: `44444444-4444-4444-4444-444444444444`
- Schedule: Monday, Wednesday, Friday 09:00–12:00 (UTC)

## Roles

| Role | Meaning |
|------|---------|
| `Admin` | Verify registrations, create users/doctors, see all appointments |
| `User` | Patient — self-register, book appointments, see own history |
| `Doctor` | See / cancel own appointments only |

## API endpoints

### POST `/api/auth/register` (anonymous)

Self-registration as patient (`User`). Account starts as `Pending` and returns a 4-digit verification code for admin approval.

```json
{ "name": "Jane Doe", "email": "jane@example.com", "password": "Password123!" }
```

### POST `/api/auth/login`

Returns a JWT with role (and `doctor_id` claim for doctors). Pending accounts cannot log in.

### GET `/api/admin/registrations` (Admin)

Lists pending registrations.

### POST `/api/admin/registrations/verify` (Admin)

Looks up the pending user by the 4-digit code (no `userId` needed).

```json
{ "code": "4821" }
```

### POST `/api/admin/users` (Admin)

Create a patient account (optionally active immediately).

### POST `/api/admin/doctors` (Admin)

Create a doctor profile + login account.

```json
{
  "name": "Dr. Jane Smith",
  "email": "doctor2@healthcare.local",
  "password": "Password123!",
  "timeZoneId": "UTC",
  "workingSchedules": [
    { "dayOfWeek": "Monday", "startTime": "09:00", "endTime": "12:00" }
  ]
}
```

### GET `/api/doctors` (Bearer JWT)

Returns all doctors with working schedules (for booking). Available to authenticated users.

### GET `/api/doctors/{id}/availability?from={iso}&to={iso}&slot=15|30|60`

Anonymous. Returns available slots excluding booked appointments.

### GET `/api/appointments` (Bearer JWT)

Scoped by role:
- **Admin** — all appointments
- **Doctor** — appointments for linked doctor only
- **User** — own patient history (`PatientId` = user Id)

### POST `/api/appointments` (Bearer JWT)

**User (patient)** — `patientId` is taken from the JWT automatically:

```json
{
  "doctorId": "11111111-1111-1111-1111-111111111111",
  "start": "2026-07-20T09:30:00Z",
  "duration": 30
}
```

**Admin** — must supply `patientId`:

```json
{
  "doctorId": "11111111-1111-1111-1111-111111111111",
  "patientId": "44444444-4444-4444-4444-444444444444",
  "start": "2026-07-20T09:30:00Z",
  "duration": 30
}
```

Doctors cannot create appointments.

### DELETE `/api/appointments/{id}` (Bearer JWT)

Cancels if ≥ 2 hours remain before start. Admin can cancel any; User/Doctor only their own.

## Core rules implemented

- No overlapping appointments (range validation + serializable transactions)
- 5-minute time alignment with nearest rounding on booking
- Valid durations: 15, 30, 60 minutes
- UTC storage with `DateTimeOffset` on API boundary
- `RowVersion` optimistic concurrency on appointments
- FluentValidation + global exception middleware
- JWT authentication with role-based authorization
- Self-registration with admin 4-digit code verification

## Tests

```bash
dotnet test HealthcareScheduling.sln
```

## Example availability request

```http
GET /api/doctors/11111111-1111-1111-1111-111111111111/availability?from=2026-07-20T08:00:00Z&to=2026-07-20T13:00:00Z&slot=30
```

Expected slots: `09:00`, `09:30`, `10:00`, `10:30`, `11:00`, `11:30`.

---

# 2. Order & Inventory API

API for order booking with real-time stock reservation, idempotent mock payments, cancellation, and simulated domain events.

## Solution structure

```
src/
  OrderInventory.Api/            # Controllers, middleware, Serilog, Swagger, seeding
  OrderInventory.Application/    # CQRS handlers, validators, DTOs, AutoMapper
  OrderInventory.Domain/         # Entities, enums, repository interfaces, exceptions
  OrderInventory.Persistence/    # EF Core, UPDLOCK reservations, RowVersion, migrations
  OrderInventory.Infrastructure/ # Mock payment gateway, logging event publisher
tests/
  OrderInventory.UnitTests/
  OrderInventory.IntegrationTests/
```

## Run the API

```bash
dotnet run --project src/OrderInventory.Api --launch-profile http
```

Swagger UI: `http://localhost:5021/swagger`

Connection string: `src/OrderInventory.Api/appsettings.Development.json`

Migrations apply automatically on startup (non-Testing). Seed inventory:

| SKU | Actual | Reserved |
|-----|--------|----------|
| A1  | 10     | 0        |
| B2  | 10     | 0        |

## Endpoints

### `POST /orders`

Create order and reserve stock atomically (all-or-nothing).

```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "items": [
    { "sku": "A1", "qty": 2 }
  ]
}
```

- Success → `201 Created`, status `PLACED`, `reservedQty` increases, `actualQty` unchanged.
- Insufficient stock → `400 Bad Request` with per-SKU details; **no** partial reservation.

Use the returned `id` for pay/cancel (not Swagger’s example GUID).

### `POST /orders/{id}/pay`

Mock payment gateway. **Idempotent** on `paymentExternalId`.

```json
{ "paymentExternalId": "XYZ-123" }
```

- First success → status `PAID`, commit reservation (`actualQty`↓, `reservedQty`↓), publish `OrderPaid` once.
- Same `paymentExternalId` again → `200 OK` with `idempotentReplay: true`; no status change, no duplicate event.
- Same id used on another order → `409 Conflict`.

### `POST /orders/{id}/cancel`

| Current status | Result |
|----------------|--------|
| `PLACED` | `CANCELLED`, release `reservedQty`, publish `OrderCancelled` |
| `PAID` | **`409 Conflict`** (see policy below) |
| `SHIPPED` | `409 Conflict` |
| `CANCELLED` | `200` idempotent replay |

### `GET /inventory/{sku}`

Returns `actualQty`, `reservedQty`, and `availableQty` (`actual - reserved`).

## Stock rules

1. **Create order** → `reservedQty += qty` (requires `actualQty - reservedQty >= qty`).
2. **Pay** → `actualQty -= qty`, `reservedQty -= qty`.
3. **Cancel (PLACED)** → `reservedQty -= qty`.

Concurrency: inventory mutations use atomic SQL `UPDATE … WITH (UPDLOCK, ROWLOCK)` (SKU ordered) plus `RowVersion` on inventory rows. Payments are deduped with a unique index on `PaymentExternalId`.

## Cancel-after-pay policy (TC5)

**Cancel is allowed only while the order is `PLACED`.**

Orders in `PAID` or `SHIPPED` return **`409 Conflict`**. Refund + restock after payment is a separate compensation flow and is **out of scope** for this API.

## Events (simulated)

Published via `IOrderEventPublisher` (default: Serilog):

- `OrderPaid` — after a non-idempotent successful pay
- `OrderCancelled` — after a non-idempotent cancel from `PLACED`

## Tests

```bash
dotnet test OrderInventory.sln
```

Integration tests expect SQL Server at `localhost,1433` (database `OrderInventoryDb_Test`). Covered cases:

1. Single-item create + reserve  
2. Multi-item insufficient stock (no partial reserve)  
3. Idempotent pay + single `OrderPaid` event  
4. Cancel before ship restores reserved  
5. Cancel after pay → 409  
6. 100 concurrent orders against stock 10 → exactly 10 reserved  
7. Inventory query  

## Load / throughput notes

- Keep transactions short; locks are per-SKU row.
- Prefer unique `PaymentExternalId` from clients for safe retries.
- Event publishing is in-process logging (no external I/O on the hot path).
