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
docker compose up -d
dotnet ef database update -p FlightControl.Dal -s FlightControl.Api
dotnet run --project FlightControl.Api
```

## Swagger Address

http://localhost:5288/swagger/index.html