# Manual test plan: permit request flow in Swagger

An end-to-end walkthrough of the permit flow. An authority defines a restricted zone, a pilot asks for flights, and the authority decides. Every step lists the request body and the expected status code. "Check" lines say what to point at in the response.

## Before you start

1. Start the database, apply the migrations and run the API (README, "Quick start"):

   ```bash
   docker compose up -d
   dotnet ef database update -p FlightControl.Dal -s FlightControl.Api
   dotnet run --project FlightControl.Api
   ```

2. Open http://localhost:5288/swagger/index.html.
3. You need the seeded authority's email and password (`Seed:AuthorityEmail`, `Seed:AuthorityPassword` in User Secrets).

**Switching users.** Swagger holds one token at a time. To act as someone, click **Authorize**, paste that user's `tokens.accessToken` (without the `Bearer ` prefix), and click **Authorize** again. Access tokens live for 15 minutes; if a call suddenly returns 401 with an empty body, log in again.

**Dates.** The flights below are on 2026-12-15 and 2026-12-16. Requests must start in the future, so if you run this plan later, move every date forward by the same number of days.

**Re-running.** The email, registration number and license number must be unique. On a database that already has them, add a suffix (for example `pilot2@demo.test`, `TC-DEMO02`, `LIC-DEMO-002`).

**Write down as you go:** `AUTHORITY_TOKEN`, `PILOT_TOKEN`, `ZONE_ID`, `AIRCRAFT_ID`, `LICENSE_ID`, `PERMIT_1_ID`, `PERMIT_2_ID`, `PERMIT_3_ID`.

## The scenario at a glance

| Item | Value |
| --- | --- |
| Restricted zone | Square 41.00 to 41.02 N, 29.00 to 29.02 E, 0 to 1000 m AGL |
| Request 1 | Square 41.01 to 41.03 N, 29.01 to 29.03 E: crosses the zone, Airspace check fails |
| Request 2 | Square 41.10 to 41.12 N, 29.10 to 29.12 E: outside the zone |
| Request 3 | Same area as request 2, one day later, for the rejection |
| Aircraft maintenance | Due 2026-12-18, within 7 days after every flight, so the Aircraft check always **warns**. This is the warning used in step 12. |

---

## 1. Authority logs in

`POST /api/auth/login` (no token needed)

```json
{
  "email": "<Seed:AuthorityEmail>",
  "password": "<Seed:AuthorityPassword>"
}
```

**Expect 200.** Check: `user.role` is `"Authority"`. Save `tokens.accessToken` as `AUTHORITY_TOKEN` and authorize Swagger with it.

## 2. Authority creates a restricted zone

`POST /api/airspace-zones` (as the authority)

```json
{
  "name": "Demo Restricted Zone",
  "type": "Restricted",
  "area": [
    { "latitude": 41.00, "longitude": 29.00 },
    { "latitude": 41.00, "longitude": 29.02 },
    { "latitude": 41.02, "longitude": 29.02 },
    { "latitude": 41.02, "longitude": 29.00 }
  ],
  "minAltitudeMeters": 0,
  "maxAltitudeMeters": 1000,
  "activeFrom": null,
  "activeUntil": null
}
```

**Expect 201.** Check: `area` has 5 points. The ring was closed automatically, so the last point equals the first. Save `id` as `ZONE_ID`.

Optional: `GET /api/airspace-zones/at-point?latitude=41.01&longitude=29.01&altitudeMeters=100` returns **200** with this zone in the list.

## 3. Pilot registers

`POST /api/auth/register` (no token needed)

```json
{
  "email": "pilot@demo.test",
  "password": "DemoPilot123",
  "fullName": "Demo Pilot",
  "role": 0
}
```

**Expect 201.** Check: `role` is `"Pilot"` and `status` is `"Active"`.

## 4. Pilot logs in

`POST /api/auth/login`

```json
{
  "email": "pilot@demo.test",
  "password": "DemoPilot123"
}
```

**Expect 200.** Save `tokens.accessToken` as `PILOT_TOKEN` and authorize Swagger with it.

## 5. Pilot adds an aircraft

`POST /api/aircraft` (as the pilot)

```json
{
  "registrationNumber": "TC-DEMO01",
  "category": "UAV",
  "maxAltitudeMeters": 500,
  "nextMaintenanceDate": "2026-12-18T00:00:00Z",
  "fleetId": null
}
```

**Expect 201.** Check: `status` is `"Active"`. Save `id` as `AIRCRAFT_ID`.

## 6. Pilot adds a license

`POST /api/licenses` (as the pilot)

```json
{
  "licenseNumber": "LIC-DEMO-001",
  "expiresAt": "2028-01-01T00:00:00Z",
  "categories": ["UAV"]
}
```

**Expect 201.** Check: `status` is `"Valid"` and `categories` is `["UAV"]`. Save `id` as `LICENSE_ID`.

## 7. Pilot requests a flight that crosses the restricted zone

`POST /api/permits` (as the pilot)

```json
{
  "aircraftId": "<AIRCRAFT_ID>",
  "licenseId": "<LICENSE_ID>",
  "area": [
    { "latitude": 41.01, "longitude": 29.01 },
    { "latitude": 41.01, "longitude": 29.03 },
    { "latitude": 41.03, "longitude": 29.03 },
    { "latitude": 41.03, "longitude": 29.01 }
  ],
  "plannedStart": "2026-12-15T10:00:00Z",
  "plannedEnd": "2026-12-15T11:00:00Z",
  "maxAltitudeMeters": 120,
  "purpose": "Survey flight over the restricted area"
}
```

**Expect 201.** Creating a request never fails because of the pre-checks; they only produce a report. Check:

- `type` is `"Individual"` and `status` is `"Pending"`.
- `checks`:

  | ruleName | outcome | description |
  | --- | --- | --- |
  | Aircraft | Warning | Maintenance is due within 7 days after the flight. |
  | Airspace | **Failed** | The area enters the restricted zone 'Demo Restricted Zone'. |
  | Conflict | Passed | No conflicting approved flights. |
  | License | Passed | The license is valid for this flight. |

Save `id` as `PERMIT_1_ID`.

## 8. Authority tries to approve it and is refused

Authorize Swagger with `AUTHORITY_TOKEN`.

`POST /api/permits/{PERMIT_1_ID}/approve`

```json
{
  "note": "Approving anyway"
}
```

**Expect 409.** Check: `detail` is "The request fails one or more pre-checks and can't be approved." A note cannot override a failed check.

Then call `GET /api/permits/{PERMIT_1_ID}`. **Expect 200** with `status` still `"Pending"`. The checks were run again on approval, and that report is the one stored.

## 9. Pilot cancels the request

Authorize Swagger with `PILOT_TOKEN`.

`POST /api/permits/{PERMIT_1_ID}/cancel` (no body)

**Expect 200.** Check: `status` is `"Cancelled"` and `cancelledAt` is set.

Optional: cancel it again. **Expect 409** ("Only pending requests can be cancelled; this request is Cancelled.").

## 10. Pilot requests a second flight outside the zone

`POST /api/permits` (as the pilot)

```json
{
  "aircraftId": "<AIRCRAFT_ID>",
  "licenseId": "<LICENSE_ID>",
  "area": [
    { "latitude": 41.10, "longitude": 29.10 },
    { "latitude": 41.10, "longitude": 29.12 },
    { "latitude": 41.12, "longitude": 29.12 },
    { "latitude": 41.12, "longitude": 29.10 }
  ],
  "plannedStart": "2026-12-15T10:00:00Z",
  "plannedEnd": "2026-12-15T11:00:00Z",
  "maxAltitudeMeters": 120,
  "purpose": "Survey flight outside the restricted area"
}
```

**Expect 201.** Check: `status` is `"Pending"`. In `checks`, Airspace is **Passed**, Aircraft is **Warning** (maintenance), and License and Conflict are Passed. Save `id` as `PERMIT_2_ID`.

## 11. Authority approves without a note and is refused

Authorize Swagger with `AUTHORITY_TOKEN`.

`POST /api/permits/{PERMIT_2_ID}/approve`

```json
{
  "note": null
}
```

**Expect 400.** Check: `errors.Note` is `["The request has warnings; a note is required to approve it."]`.

## 12. Authority approves with a note

`POST /api/permits/{PERMIT_2_ID}/approve`

```json
{
  "note": "Maintenance warning accepted; the flight is short and well before the due date."
}
```

**Expect 200.** Check: `status` is `"Approved"`, `decisionNote` is the note above, and `decidedById` and `decidedAt` are set.

Optional: approve it again. **Expect 409** ("Only pending requests can be approved; this request is Approved.").

## 13. Pilot requests a third flight, and the authority rejects it

Authorize Swagger with `PILOT_TOKEN`.

`POST /api/permits`: same body as step 10, with these changes:

```json
{
  "plannedStart": "2026-12-16T10:00:00Z",
  "plannedEnd": "2026-12-16T11:00:00Z",
  "purpose": "Second survey flight"
}
```

**Expect 201.** It is a different day from the approved request 2, so the Conflict check passes. Save `id` as `PERMIT_3_ID`.

Authorize Swagger with `AUTHORITY_TOKEN`.

First, reject without a reason: `POST /api/permits/{PERMIT_3_ID}/reject`

```json
{
  "reason": ""
}
```

**Expect 400.** Check: `errors.Reason` is `["A reason is required to reject a request."]`.

Then reject with a reason:

```json
{
  "reason": "Two survey flights on consecutive days are not needed; combine them into one request."
}
```

**Expect 200.** Check: `status` is `"Rejected"`, `decisionNote` is the reason, and `decidedAt` is set.

## 14. Pilot views the decisions

Authorize Swagger with `PILOT_TOKEN`.

- `GET /api/permits/{PERMIT_2_ID}`: **expect 200** with `status` `"Approved"` and the authority's note.
- `GET /api/permits/{PERMIT_3_ID}`: **expect 200** with `status` `"Rejected"` and the reason in `decisionNote`.
- `GET /api/permits`: **expect 200** with all three requests (Cancelled, Approved, Rejected) in `items` and `totalCount` 3.
- `GET /api/permits?status=Approved`: **expect 200** with only request 2.

## Optional extras

| Step | Call | Expect |
| --- | --- | --- |
| Pilot deletes the aircraft used by the requests | `DELETE /api/aircraft/{AIRCRAFT_ID}` as the pilot | **409** "This aircraft has permit requests and can't be deleted." |
| Pilot deletes the license used by the requests | `DELETE /api/licenses/{LICENSE_ID}` as the pilot | **409** "This license has permit requests and can't be deleted." |
| Pilot tries to approve | `POST /api/permits/{PERMIT_3_ID}/approve` as the pilot | **403** (empty body; role check) |
| Authority lists military requests first | `GET /api/permits?sort=militaryFirst` as the authority | **200** |
| Another user reads the pilot's request | `GET /api/permits/{PERMIT_2_ID}` as a different pilot | **404** (requests of others look like they don't exist) |
