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

### 4. JWT signing key

Access tokens are signed with a secret key of at least 32 bytes. Generate a random one:

```powershell
# Windows PowerShell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

```bash
# Linux / macOS
openssl rand -base64 64
```

Add it to User Secrets:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<generated key>" --project FlightControl.Api
```

If the key is missing or too short, the application does not start.

### 5. Run

```bash
docker compose up -d
dotnet ef database update -p FlightControl.Dal -s FlightControl.Api
dotnet run --project FlightControl.Api
```

## Swagger Address

http://localhost:5288/swagger/index.html

## Endpoints

"Authenticated" means any user with a valid access token (`Authorization: Bearer <token>`). Role-restricted endpoints return 403 for other roles.

### Auth

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/auth/register` | Anonymous |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/refresh` | Anonymous (refresh token in body) |
| POST | `/api/auth/logout` | Anonymous (refresh token in body) |
| POST | `/api/auth/change-password` | Authenticated |

Accounts registered with the `Military` role cannot log in until the authority approves them.

### Users

| Method | Route | Required role |
| --- | --- | --- |
| GET | `/api/users/me` | Authenticated |
| GET | `/api/users/pending` | Authority |
| POST | `/api/users/{id}/approve` | Authority |
| POST | `/api/users/{id}/suspend` | Authority |
| POST | `/api/users/{id}/reactivate` | Authority |

### Aircraft

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/aircraft` | Pilot, FleetOwner, Military |
| GET | `/api/aircraft` | Authenticated (owners see their own, the authority sees all) |
| GET | `/api/aircraft/{id}` | Authenticated (owner or authority) |
| PUT | `/api/aircraft/{id}` | Pilot, FleetOwner, Military (owner only) |
| POST | `/api/aircraft/{id}/status` | Authenticated (owner: Active/Maintenance; authority: any, including Grounded) |
| DELETE | `/api/aircraft/{id}` | Pilot, FleetOwner, Military (owner only) |

### Fleets

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/fleets` | FleetOwner, Military |
| GET | `/api/fleets` | Authenticated (owners see their own, the authority sees all) |
| GET | `/api/fleets/{id}` | Authenticated (owner or authority) |
| DELETE | `/api/fleets/{id}` | FleetOwner, Military (owner only) |
| POST | `/api/fleets/{id}/members` | FleetOwner, Military (owner only) |
| GET | `/api/fleets/{id}/members` | Authenticated (owner or authority) |
| DELETE | `/api/fleets/{id}/members/{pilotId}` | FleetOwner, Military (owner only) |

### Health

| Method | Route | Required role |
| --- | --- | --- |
| GET | `/api/health` | Anonymous |
| GET | `/api/health/error?type=...` | Anonymous (test endpoint for error responses) |