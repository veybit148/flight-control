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

## Running the tests

```bash
dotnet test
```

Run it from the repository root. The tests (xUnit, FluentAssertions) cover the permit pre-check rules, permit state transitions and the geometry helpers; they need neither Docker nor a database.

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

### Licenses

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/licenses` | Pilot |
| GET | `/api/licenses` | Authenticated (pilots see their own, the authority sees all) |
| GET | `/api/licenses/{id}` | Authenticated (owner or authority) |
| PUT | `/api/licenses/{id}` | Pilot (owner only; expiry and categories, the number cannot change) |
| DELETE | `/api/licenses/{id}` | Pilot (owner only; 409 if any permit request uses it) |
| POST | `/api/licenses/{id}/suspend` | Authority |
| POST | `/api/licenses/{id}/reinstate` | Authority |

Deleting an aircraft that is used by any permit request also returns 409.

### Airspace zones

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/airspace-zones` | Authority |
| GET | `/api/airspace-zones` | Authenticated |
| GET | `/api/airspace-zones/{id}` | Authenticated |
| PUT | `/api/airspace-zones/{id}` | Authority |
| DELETE | `/api/airspace-zones/{id}` | Authority |
| GET | `/api/airspace-zones/at-point?latitude=...&longitude=...&altitudeMeters=...&at=...` | Authenticated |

### Permits

| Method | Route | Required role |
| --- | --- | --- |
| POST | `/api/permits` | Pilot, FleetOwner, Military (type follows the role: Individual, Fleet, Military) |
| GET | `/api/permits?status=&type=&from=&to=&sort=&page=&pageSize=` | Authenticated (requesters see their own, the authority sees all; `sort=militaryFirst`) |
| GET | `/api/permits/{id}` | Authenticated (requester or authority) |
| POST | `/api/permits/{id}/cancel` | Pilot, FleetOwner, Military (requester only, while pending) |
| POST | `/api/permits/{id}/approve` | Authority (409 if a pre-check fails; a note is required if one warns) |
| POST | `/api/permits/{id}/reject` | Authority (reason required) |

Pre-checks (license, aircraft, airspace, conflicts) run when a request is created and again when it is approved; the latest report is returned in `checks`.

### Health

| Method | Route | Required role |
| --- | --- | --- |
| GET | `/api/health` | Anonymous |
| GET | `/api/health/error?type=...` | Anonymous (test endpoint for error responses) |