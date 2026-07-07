# MoneyTransfer

A full-stack money transfer app: an ASP.NET Core Web API backed by SQL Server, a React (Vite) frontend, and Docker Compose to run the whole thing with one command.

## Project layout

```
backend/MoneyTransfer.Api/
  Controllers/         Thin HTTP layer — routes requests to services, maps results to status codes
  Services/
    Accounts/           Read access to accounts
    Transactions/        Read access to transaction history
    Transfers/            Transfer business logic (validation + atomic balance updates)
  Data/                 ApplicationDbContext (EF Core)
  Models/               Account, Transaction entities
  Dtos/                 Request/response shapes exposed over the API
  Middleware/           Global exception handling
  Migrations/           EF Core migrations
  Dockerfile            Multi-stage build: SDK compiles the API, ASP.NET runtime serves it

frontend/
  Dockerfile            Multi-stage build: Node builds the Vite app, nginx serves the static output
  src/
    api/                 fetch wrappers per resource (accountsApi, transactionsApi, transferApi)
    hooks/                useAccounts, useTransactions — data fetching + loading/error/refetch state
    components/           AccountsList, TransactionsList, TransferForm
    config/               apiConfig.js — API base URL, driven by VITE_API_BASE_URL

docker-compose.yml      Defines all three services: sqlserver, api, frontend
```

Controllers never talk to the database directly — they call into `Services`, which use `ApplicationDbContext` from `Data`. This keeps the transfer rules (sufficient balance, atomicity, audit logging) in one testable place: `TransferService.ExecuteTransferAsync`.

## Running the whole stack with Docker (recommended)

```
docker compose up --build
```

One command builds and starts all three containers:

| Service     | Container                | Host port | What it is                                             |
|-------------|--------------------------|-----------|---------------------------------------------------------|
| `sqlserver` | moneytransfer-sqlserver  | `1433`    | SQL Server 2022 (replaces LocalDB, which can't run in a Linux container) |
| `api`       | moneytransfer-api        | `8080`    | ASP.NET Core Web API, Swagger UI at `/swagger`           |
| `frontend`  | moneytransfer-frontend   | `3000`    | The built React app, served as static files by nginx    |

Open **`http://localhost:3000`** in a browser for the app, or hit the API directly at `http://localhost:8080/api/accounts`.

No manual database step is needed: the API applies EF Core migrations automatically on startup, creating `MoneyTransferDb` on the `sqlserver` container and seeding it with two accounts, "Alice" and "Bob", the first time it starts.

**How the pieces find each other:**
- `api` → `sqlserver`: the compose file overrides `ConnectionStrings__DefaultConnection` as an environment variable pointing at `Server=sqlserver,1433;...` — the container-network service name, not `localhost`. This only takes effect inside Docker; it doesn't touch `appsettings.json`, so running the API outside Docker still uses whatever connection string is configured there for local development.
- Browser → `api`: the browser runs outside the Docker network entirely, so it uses the *host-mapped* port (`http://localhost:8080/api`), not the `api` service name — service names only resolve for container-to-container calls.
- CORS: the API's allowed origins already include `http://localhost:3000`, so the containerized frontend can call the containerized API with no extra configuration.

**Rebuilding after a config change:** Vite bakes `VITE_API_BASE_URL` into the JS bundle at *build* time, not at container start. If you change the `args.VITE_API_BASE_URL` value in `docker-compose.yml`, rebuild that image (`docker compose up --build frontend`) — restarting the container alone won't pick up the new value.

To stop everything: `docker compose down` (add `-v` to also delete the SQL Server data volume and start fresh next time).

## Running the backend without Docker

Requires the .NET 10 SDK and a reachable SQL Server instance (SQL Server, LocalDB, or Docker).

```
cd backend/MoneyTransfer.Api
dotnet ef database update     # applies migrations
dotnet run
```

Set `ConnectionStrings:DefaultConnection` in `appsettings.json` (or override it with the `ConnectionStrings__DefaultConnection` environment variable) to point at your SQL Server instance. Check `Properties/launchSettings.json` for which port it starts on.

## Running the frontend without Docker

```
cd frontend
npm install
npm run dev
```

The dev server runs at `http://localhost:5173`, already in the backend's CORS allow-list. It reads the API's base URL from `frontend/.env` (`VITE_API_BASE_URL`) — make sure this matches whatever host/port your locally-running API is actually listening on.

## API endpoints

| Method | Route                | Description                                                     |
|--------|-----------------------|-------------------------------------------------------------------|
| GET    | `/api/accounts`       | List all accounts with current balances                          |
| GET    | `/api/transactions`   | List all transactions (success and failed), most recent first    |
| POST   | `/api/transfer`       | Transfer money between two accounts                               |

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
- New frontend feature: add a fetch function under `frontend/src/api/`, a hook under `frontend/src/hooks/` if it needs loading/error state, and a component under `frontend/src/components/`. `App.jsx` is the only place that wires hooks to components together.
