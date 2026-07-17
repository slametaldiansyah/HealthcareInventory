# Healthcare Appointment & Availability API

Production-ready ASP.NET Core 8 Web API implementing Clean Architecture, CQRS (MediatR), JWT authentication, AutoMapper, and SQL Server for doctor appointment scheduling with strict overlap prevention and concurrency control.

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

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Docker example below)

## Database setup

Start SQL Server with Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

Update the connection string in `src/HealthcareScheduling.Api/appsettings.Development.json` if needed.

## Run migrations

Migrations are applied automatically on startup via `DbSeeder`. To create or update migrations manually:

```bash
dotnet ef migrations add InitialCreate --project src/HealthcareScheduling.Persistence --startup-project src/HealthcareScheduling.Api
dotnet ef database update --project src/HealthcareScheduling.Persistence --startup-project src/HealthcareScheduling.Api
```

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
