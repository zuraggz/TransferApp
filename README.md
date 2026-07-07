# MoneyTransfer

Full-stack money transfer app: ASP.NET Core Web API + SQL Server + React (frontend added in a later phase) + Docker.

This README covers **Phase 1**: the backend API.

## Project layout

```
backend/MoneyTransfer.Api/
  Controllers/        Thin HTTP layer — routes requests to services, maps results to status codes
  Services/
    Accounts/          Read access to accounts
    Transactions/       Read access to transaction history
    Transfers/           Transfer business logic (validation + atomic balance updates)
  Data/                ApplicationDbContext (EF Core)
  Models/              Account, Transaction entities
  Dtos/                Request/response shapes exposed over the API
  Middleware/          Global exception handling
  Migrations/          EF Core migrations
docker-compose.yml     SQL Server + API, for local development
```

Controllers never talk to the database directly — they call into `Services`, which use `ApplicationDbContext` from `Data`. This keeps the transfer business rules (sufficient balance, atomicity, audit logging) in one testable place: [TransferService.cs](backend/MoneyTransfer.Api/Services/Transfers/TransferService.cs).

## Running locally with Docker (recommended)

```
docker-compose up --build
```

This starts SQL Server and the API (which applies EF Core migrations automatically on startup, including two seed accounts, "Alice" and "Bob"). The API is available at `http://localhost:8080`, with Swagger UI at `http://localhost:8080/swagger` in the Development environment.

> Note: this compose setup was authored but not verified in this sandbox (no Docker available here) — please confirm it on your machine and report back if anything needs adjusting.

## Running locally without Docker

Requires the .NET 10 SDK and a reachable SQL Server instance (SQL Server, LocalDB, or Docker).

```
cd backend/MoneyTransfer.Api
dotnet ef database update     # applies migrations
dotnet run
```

Update the `ConnectionStrings:DefaultConnection` value in `appsettings.json` (or override it with the `ConnectionStrings__DefaultConnection` environment variable) to point at your SQL Server instance.

## API endpoints

| Method | Route              | Description                                   |
|--------|--------------------|------------------------------------------------|
| GET    | `/api/accounts`     | List all accounts with current balances       |
| GET    | `/api/transactions` | List all transactions (success and failed), most recent first |
| POST   | `/api/transfer`      | Transfer money between two accounts            |

### POST /api/transfer

Request body:
```json
{ "fromAccountId": 1, "toAccountId": 2, "amount": 100.00 }
```

Responses:
- `200 OK` — transfer succeeded; returns the transaction id and both accounts' updated balances.
- `400 Bad Request` — invalid amount, same account on both sides, or insufficient balance.
- `404 Not Found` — one of the account ids doesn't exist.
- `409 Conflict` — the account was concurrently modified by another operation; retry.

Every transfer attempt — including failed ones — is recorded in the `Transactions` table with a `Status` and, on failure, a `Reason`, so `/api/transactions` doubles as an audit log.

## Extending this

- New read-only endpoint: add a method to the relevant service interface (e.g. `IAccountService`), implement it, and expose it from the controller.
- New business rule for transfers: add it to `TransferService.ExecuteTransferAsync` — it already runs inside a single database transaction, so any additional multi-step change is automatically atomic.
- New entity: add a `DbSet<T>` and any relationship configuration to `ApplicationDbContext`, then run `dotnet ef migrations add <Name>`.
- Account balances carry a `RowVersion` concurrency token, so concurrent transfers against the same account are detected and reported as `409 Conflict` rather than silently corrupting the balance.
