# Order & Inventory API

ASP.NET Core 8 Web API for order booking with real-time stock reservation, idempotent mock payments, cancellation, and simulated domain events. Built with Clean Architecture + CQRS (MediatR), matching the Healthcare Scheduling solution style.

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

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Docker example below)

## Database setup

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

Connection string (Development): `src/OrderInventory.Api/appsettings.Development.json`

## Run the API

```bash
dotnet run --project src/OrderInventory.Api --launch-profile http
```

Swagger UI: `http://localhost:5021/swagger`

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
