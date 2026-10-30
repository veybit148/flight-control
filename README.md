# Flight Control

**Airspace control and management system**

## Requirements

.NET 10 SDK, Docker

## Quick start

Docker **must** be running

```bash
git clone https://github.com/veybit148/flight-control.git
cd flight-control
dotnet tool restore
```

### 1. Database settings
Copy the example environment file and fill in your own values:

```bash
cp .env.example .env
```

On Windows PowerShell, use `Copy-Item .env.example .env`.

### 2. Connection string

The connection string is not stored in the repository. Add it to User Secrets, using the same user, password and database name as in `.env`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" --project FlightControl.Api
```

### 3. Authority account

The first authority account is created on startup from configuration. Add its credentials to User Secrets:

```bash
dotnet user-secrets set "Seed:AuthorityEmail" "<email>" --project FlightControl.Api
dotnet user-secrets set "Seed:AuthorityPassword" "<password>" --project FlightControl.Api
```

If these are missing, the application starts without an authority account and logs a warning.

### 4. Run

```bash
docker compose up -d
dotnet ef database update -p FlightControl.Dal -s FlightControl.Api
dotnet run --project FlightControl.Api
```

## Swagger Address

http://localhost:5288/swagger/index.html