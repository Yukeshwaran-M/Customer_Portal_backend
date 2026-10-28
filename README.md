# Customer Operations API (.NET + PostgreSQL)

ASP.NET Core Web API, EF Core + Npgsql, Swagger. Serves the Angular app at http://localhost:4200.

See the setup steps in the chat instructions; short version:

```bash
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run          # http://localhost:5000/swagger
```

## Endpoints
| Method | URL | Purpose |
|---|---|---|
| GET | /api/customers?search&status&city&sortBy&sortDirection&page&pageSize | search / filter / sort / page |
| GET | /api/customers/summary | dashboard counts + cities |
| GET | /api/customers/email-exists?email=&excludeId= | duplicate-email check |
| GET | /api/customers/{id} | details |
| POST | /api/customers | create |
| PUT | /api/customers/{id} | update |
| PATCH | /api/customers/{id}/status | activate / deactivate |

## Reproducing errors (appsettings.json -> MockBehavior)
| Case | How |
|---|---|
| Failed list request | search `error` (503) |
| Empty result | search `zzz` |
| Failed save | save with email `fail@test.com` (500) |
| Failed status change | change status of `CUS-1008` (500) |
| Duplicate email | any existing email, any case/whitespace (409) |
| Delays | MinDelayMs / MaxDelayMs; `Enabled: false` turns all simulation off |

Error body for every failure: `{ "message": "...", "code": "DUPLICATE_EMAIL | NOT_FOUND | SERVER_ERROR | VALIDATION" }`.
