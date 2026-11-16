# Design Decisions

Design decisions made in this project, including places where it intentionally deviates from the original task document, and why.

## 1. Unit of Work instead of a save method on the base repository

**Date:** 2026-10-22

**Decision:** Saving changes is done through a separate `IUnitOfWork` with a single `SaveChangesAsync` method. Repositories only mark changes; they never save.

**Reason:** Saving must be triggered by the Operation, so that several changes made in one request are written together or not at all. A `SaveChangesAsync` method on every repository would work, but it is misleading: `_aircraftRepository.SaveChangesAsync()` reads as "save the aircraft", while it actually saves every pending change in the request. A separate Unit of Work makes this explicit, at the cost of only one interface, one class and one DI registration.

## 2. `Add` instead of `AddAsync`

**Date:** 2026-10-22

**Decision:** `IBaseRepository<T>.Add` is synchronous, like `Update` and `Delete`.

**Reason:** Adding an entity does not hit the database; it only marks the entity as added in the change tracker. EF Core's `AddAsync` exists only for value generators that need a database round trip to produce keys. Our IDs are generated in code with `Guid.CreateVersion7()`, so that case never happens and the async version adds nothing.

## 3. User role stored as text

**Date:** 2026-10-26

**Decision:** `User.Role` is stored as a string column (`HasConversion<string>()`), not as the enum's integer value.

**Reason:** Integer values depend on the order of the enum members. Inserting a new role at the top of the enum would silently shift every stored value and change existing users' roles. Text values are also readable directly in the database. The extra storage is negligible at this scale.

## 4. Separate operations for authentication and user data

**Date:** 2026-10-27

**Decision:** Registration and login live in `AuthOperation`. Other user features (profile, fleet management) will live in a separate `UserOperation`.

**Reason:** The two groups have different dependencies: authentication needs the password service and the token generator, user data management does not. Keeping them apart keeps each constructor small and puts all security-sensitive code in one place.

## 5. Emails are normalized before storing and comparing

**Date:** 2026-10-27

**Decision:** Every email is trimmed and lowercased with `ToLowerInvariant()` before it is checked for existence, stored, or used to look up a user. Every path that writes or reads emails (registration, login, seed data) must follow this rule. The rule lives in one place, `EmailNormalizer.Normalize`.

**Reason:** The unique index on `Email` is case-sensitive, so without normalization `Ali@mail.com` and `ali@mail.com` would be two different accounts. `ToLowerInvariant()` is used instead of `ToLower()` because the latter depends on the machine's culture: on a Turkish system, `I` becomes `ı`. Lowercasing inside the query (`u.Email.ToLower() == email`) was rejected because it prevents the database from using the index.

**Known limitation:** If two registrations with the same email arrive at the same time, both may pass the existence check. The unique index rejects the second one, but this currently surfaces as a 500 instead of a 409.

## 6. Password hashing behind an adapter

**Date:** 2026-10-27

**Decision:** Operations use our own `IPasswordService` (`HashPassword`, `VerifyPassword`), implemented by `PasswordService`, which wraps ASP.NET Core's `IPasswordHasher<User>`.

**Reason:** `IPasswordHasher<User>` requires a `User` instance, but a `User` cannot be created before its password hash exists. The default implementation ignores this argument, so `PasswordService` passes `null`, in one place and with a comment, instead of spreading this detail across operations. Swapping the hashing library later only changes this class. `VerifyPassword` returns a `bool` and treats `SuccessRehashNeeded` as success, so users with hashes created under older settings can still log in.

## 7. Validators registered by their abstract base class

**Date:** 2026-10-27

**Decision:** Validators are registered in DI as `AbstractValidator<T>`; there is no separate `IValidator<T>` interface. The base class also offers `ThrowIfInvalid`, which validates and throws a `ValidationException` when the result is invalid.

**Reason:** All validators share the same flow, so an abstract base class already provides both the contract and the shared code. An extra interface would add a layer without a current benefit. Throwing on invalid input belongs to the validator, not to every operation, so each operation validates with a single call.

## 8. Two validation layers

**Date:** 2026-10-27

**Decision:** ASP.NET Core's automatic model validation (`[ApiController]`) stays enabled alongside our own validators.

**Reason:** The framework handles structural problems (missing fields, wrong types, malformed JSON) before the request reaches the controller. Our validators handle business rules on a well-formed request. The accepted trade-off: a request with a missing field gets only the framework's errors first, and the two 400 responses differ slightly in format (the framework's includes `type` and `traceId`).

## 9. Secrets kept out of the repository

**Date:** 2026-10-30

**Decision:** The connection string and the seed credentials are read from configuration, never from committed files. During development they live in .NET User Secrets; in other environments they are provided as environment variables (for example `ConnectionStrings__DefaultConnection`). Docker Compose reads the database credentials from a `.env` file, which is ignored by Git; a `.env.example` with placeholder values is committed instead. The database port is bound to `127.0.0.1` only.

**Reason:** Anything committed stays in the Git history, even after it is removed from the file. The original development password had already been committed, so it was replaced rather than only moved. Binding the port to `127.0.0.1` keeps the database unreachable from other machines on the same network.

## 10. Authority account seeded on startup

**Date:** 2026-10-30

**Decision:** The first authority account is created by `AuthoritySeeder` (in Domain), which `Program.cs` runs after `Build()` and before `Run()`, inside a manually created DI scope. `Program.cs` reads the email and password from configuration and passes them as parameters, so Domain does not depend on the configuration system. The seeder uses the same building blocks as registration: `EmailNormalizer`, `IUserRepository`, `IPasswordService` and `IUnitOfWork`. It does nothing if any authority already exists, refuses to continue (with a warning) if the email belongs to a non-authority user, and the application starts without an authority (with a warning) if the configuration values are missing.

**Reason:**

- Registration cannot create authority accounts by design, so the first one needs a separate path.
- Credentials must not be in the code.
- EF Core's migration-based seeding (`HasData`) requires fixed values, which does not fit generated IDs, timestamps and password hashes with a random salt; seeding at runtime avoids this.
- Running before `Run()` means the authority exists before the first request is accepted, so on a fresh deployment nobody can register the authority's email first.
- The seeder never upgrades an existing user to authority. Someone who registers the authority's email in advance can block the seed, but cannot gain authority rights.

**Consequence:** The application now needs the database at startup. If the database is unreachable, startup fails instead of the first request.

## 11. Registration reveals existing emails (accepted risk)

**Date:** 2026-10-30

**Decision:** Registering with an email that is already in use returns 409 with "This email is already in use."

**Reason:** This allows user enumeration: anyone can check whether an email is registered. The usual fix is a verification flow in which registration always returns the same response and the details are sent to the email address itself; this needs email infrastructure, which is outside the scope of this project. Rate limiting on the registration endpoint could make enumeration slower later. Login must not repeat this: an unknown email and a wrong password will return the same response.

## 12. Refresh token reuse revokes every session of the user

**Date:** 2026-11-10

**Decision:** Refresh tokens are rotated on every use: the used token is revoked and a new one is issued. Revoked rows are kept, not deleted. If a revoked token is presented again, it is treated as a sign that the token was copied, and every active refresh token of that user is revoked. On refresh, the revocation check runs before the expiry check, so a revoked token that has also expired is still detected as reuse.

**Reason:** A rotated token is used only once by a well-behaved client, because it receives the new token in the same response. A second use means two parties hold the same token, and the server cannot tell which one is the real user. Revoking only the affected session would leave other sessions open if the attacker also has another way in. In an airspace control system, an authority account in the wrong hands can approve flights and remove restricted zones, so asking the user to log in again everywhere is the smaller cost.

**Known trade-offs:**

- If the response of a successful refresh is lost (for example, the connection drops), the client retries with the old token and the user is logged out of every device. There is no grace period for this.
- If a client sends several refresh requests in parallel with the same token (for example, several requests receive 401 at once), every request after the first is treated as reuse. Clients must send only one refresh request at a time and let other requests wait for its result.

## 13. Concurrent refresh requests are not protected (known limitation)

**Date:** 2026-11-10

**Decision:** The refresh flow reads the token, checks that it is not revoked, and then revokes it and saves a new one. These steps are not protected against two requests running at the same time.

**Reason:** If two refresh requests with the same token arrive within a few milliseconds, both can read the row before either saves, both see it as not revoked, and both issue a new token. The session then splits into two valid chains and reuse detection does not trigger. The usual fix is optimistic concurrency: a version value on the row, so that the second save is rejected because the row changed after it was read. This is postponed until the permit approval step, where the same problem appears (two authorities deciding on the same request) and both will use the same mechanism. Until then, item 12's client rule (one refresh request at a time) also reduces the chance of this happening.

## 14. Interface in Domain, implementation in Api for the current user

**Date:** 2026-11-05

**Decision:** `ICurrentUser` (user ID, role, `IsAuthenticated`) is defined in Domain. `CurrentUser` implements it in the Api project by reading the claims of the current HTTP request through `IHttpContextAccessor`. Values are computed when read, not stored in the constructor.

**Reason:** Operations need to know who is calling, but Domain must not depend on ASP.NET Core. Passing the user ID as a parameter from every controller would let a controller pass a wrong value (for example, one from the request body). With `ICurrentUser`, the identity always comes from the signed token. Values are read on access so that the class stays correct even if its lifetime is registered wrongly.

## 15. Two sources of 401 responses

**Date:** 2026-11-05

**Decision:** `UnauthorizedException` is the base type for every 401 raised by our code; `InvalidCredentialsException` derives from it. The middleware maps the base type to a 401 with a `ProblemDetails` body.

**Reason:** A single catch handles every current and future 401 case. The accepted inconsistency: 401 responses produced by `JwtBearer` (missing, expired or invalid access token) have an empty body and carry the reason in the `WWW-Authenticate` header, while ours carry it in the body.

## 16. Refresh tokens

**Date:** 2026-11-10

**Decision:**

- Login returns a 15-minute JWT access token and a refresh token. `POST /api/auth/refresh` exchanges a refresh token for a new pair; it does not require an access token.
- A refresh token is 64 random bytes (Base64Url). Only its SHA-256 hash (hex) is stored, with a unique index. No salt and no slow hash.
- Lifetime is hybrid: each token expires 7 days after it is issued, but never later than 30 days after the login that started the session. Both values come from `RefreshTokenOptions` and are validated on startup.
- Every login starts a new session; earlier sessions are not revoked.
- Refresh checks, in this order: the token exists, it is not revoked (otherwise item 12), it has not expired, the user still exists and is `Active` (item 27). All failures return the same message.
- Revocation sets `RevokedAt`; rows are never deleted on revocation. Deleting a user deletes their refresh tokens (cascade).

**Reason:** Short access tokens limit the damage of a stolen token but cannot be revoked; refresh tokens are stored and can be. A random 512-bit value cannot be guessed, so a fast unsalted hash is enough and allows a direct lookup by hash, which a salted hash would not. The absolute limit stops a stolen token chain from living forever.

**Known limitation:** A session abandoned by the user (for example, after logging in again on the same device) stays valid until it expires. If such a token was stolen, reuse detection never triggers because the real user never presents it again. Revoking all sessions on password change would close this gap and is not implemented yet.

**Resolved (2026-11-16):** A password change now revokes every active refresh token of the user (item 29), and a user can end a session explicitly with logout (item 28). A token stolen from an abandoned session still lives until it expires if the user never changes their password.

## 17. Fleets with a separate membership table

**Date:** 2026-11-11

**Decision:** A `Fleet` has a name and an owner. Pilots join fleets through `FleetMembership` rows, with a unique index on `(FleetId, PilotId)`. A fleet owner can have several fleets, and a pilot can be a member of several fleets. Fleet names are unique per owner, with a unique index on `(OwnerId, Name)`.

**Reason:** A pilot can work for several operators, and a military command can split its units into several fleets. A single `FleetOwnerId` on the user would not allow either.

## 18. Military commanders own fleets with the Military role

**Date:** 2026-11-11

**Decision:** Users with the `Military` role can own fleets, like fleet owners. Military personnel use the `Pilot` role and are members of military fleets. A military permit request is opened by the commander on behalf of a pilot.

**Reason:** The task document derives the permit type from the role of the requester (Pilot → Individual, FleetOwner → Fleet, Military → Military). Keeping commanders in the `Military` role keeps that rule intact. A separate `IsMilitary` flag on fleets was rejected because it would duplicate information already given by the owner's role. A pilot who opens a request on their own gets an individual request, which matches reality: military flights are authorised through the command chain.

## 19. Aircraft owner and fleet

**Date:** 2026-11-11

**Decision:** `Aircraft.OwnerId` is always set and is the user who manages the aircraft in this system (the operator, not necessarily the legal owner). `FleetId` is optional; an aircraft without a fleet is individual. An aircraft can be assigned only to a fleet of its own owner. Registration numbers are trimmed, converted to upper case, unique, and can be changed on update.

**Reason:** Ownership checks stay a single comparison (`aircraft.OwnerId == currentUserId`) regardless of fleets. An `IsIndividual` flag was rejected because it would duplicate what a null `FleetId` already says.

## 20. Aircraft status machine and history

**Date:** 2026-11-12

**Decision:** Aircraft start as `Active`. The status can be changed only through `POST /api/aircraft/{id}/status`, never through a regular update. The owner can switch between `Active` and `Maintenance`; only the authority can set or lift `Grounded`, and a reason is required when grounding. Every change is stored in `AircraftStatusChange` (from, to, reason, who, when) in the same save as the status itself. A change to the current status returns 409; a transition the user is not allowed to make returns 403.

**Reason:** In aviation, routine maintenance is the operator's responsibility, while grounding is a regulatory decision. A dedicated endpoint makes every status change pass through the same rules and leave a history row.

## 21. Access rules for aircraft and fleets

**Date:** 2026-11-12

**Decision:** Owners see and manage only their own aircraft and fleets. The authority sees all of them, can change aircraft status, and cannot create, update or delete aircraft or fleets. Accessing an aircraft or fleet that belongs to someone else returns 404, the same as one that does not exist. Role restrictions are applied on the controller with `[Authorize(Roles = ...)]`, which returns 403.

**Reason:** A 403 for someone else's resource would confirm that it exists. Role checks are coarse checks on who the caller is and belong on the endpoint; ownership checks need the data and belong in the operation.

## 22. Delete behaviors

**Date:** 2026-11-11

**Decision:**

- Users with fleets or aircraft cannot be deleted (restrict).
- Deleting a fleet deletes its memberships (cascade) and detaches its aircraft (set null).
- Deleting a user deletes their fleet memberships and refresh tokens (cascade).
- Deleting an aircraft deletes its status history (cascade).

**Reason:** Cascade is used only for rows that have no meaning without their parent. Aircraft and fleets have value of their own and later carry permit history, so their deletion must be a deliberate step.

**Known limitation:** Deleting an aircraft also deletes its status history. Retiring aircraft instead of deleting them would keep it.

## 23. Fleet names are case-sensitive

**Date:** 2026-11-11

**Decision:** Fleet names are only trimmed; "Doğu Filosu" and "doğu filosu" are different names.

**Reason:** Case-insensitive comparison depends on language rules: in Turkish the lower case of "I" is "ı", in English it is "i", and `ToLowerInvariant` turns "İ" into "i" plus a combining dot. A Turkish collation would handle Turkish names correctly but English names with "I" incorrectly. The extra complexity was not worth preventing names that differ only in case.

## 24. Enum values travel as strings in DTOs

**Date:** 2026-11-12

**Decision:** Category and status are strings in DTOs. Validators accept only exact enum names (case-insensitive); mappings convert them with `Enum.Parse`.

**Reason:** The Dto project has no dependencies and cannot see the enums in Dal. A duplicate enum in Dto would have to be kept in sync by hand. Validation by name avoids `Enum.TryParse` accepting numbers or comma-separated values.

## 25. Fleet members are added by email

**Date:** 2026-11-12

**Decision:** A fleet owner adds a member by the pilot's email address. Only users with the `Pilot` role can be added; an unknown email and a non-pilot both return 404.

**Reason:** A fleet owner knows a pilot's email, not their ID.

**Known limitations:**

- A fleet owner can find out whether an email belongs to a registered pilot.
- Pilots are added without their consent; an invitation flow is out of scope.

## 26. Open items

**Date:** 2026-11-12

- Anyone can register with the `Military` role. Planned: military accounts need authority approval before they can log in. **Resolved (2026-11-16, item 27).**
- Users have no status (pending, active, suspended); the only way to deactivate a user is to delete them. Planned together with the item above. **Resolved (2026-11-16, item 27).**
- Revoking all refresh tokens on password change (item 16). **Resolved (2026-11-16, item 29).**

## 27. User status and military account approval

**Date:** 2026-11-16

**Decision:**

- `User.Status` is one of `PendingApproval`, `Active` and `Suspended`, stored as a string like the role (item 3). The `AddUserStatus` migration gives the column the database default `'Active'`, so existing users became active.
- Users who register with the `Military` role start as `PendingApproval`; every other registration and the seeded authority start as `Active`.
- Login checks the status only after the password has been verified. A pending or suspended user gets 403 with a message saying which, and no tokens. An unknown email or a wrong password still gets the same 401 as before.
- Refresh treats a user who is not `Active` like any other invalid refresh token: the same 401 and message.
- The authority manages status through `/api/users/pending`, `/api/users/{id}/approve`, `/suspend` and `/reactivate`. Allowed transitions: `PendingApproval → Active` (approve), `PendingApproval` or `Active → Suspended` (suspend), `Suspended → Active` (reactivate). Any other transition returns 409, an unknown user 404, and changing one's own status 403.
- Suspending a user revokes all of their active refresh tokens in the same save as the status change.
- `UserResponse` carries the status as a string (item 24).
- The database default exists only in the migration, not in the EF model (`HasDefaultValue`). `PendingApproval` is the enum's CLR default (0), so with a model default EF would treat a pending user as "value not set", leave the column out of the `INSERT`, and the database would store `Active`. Every insert therefore sends the status explicitly; `User.Status` is `required`, so it cannot be forgotten.

**Reason:** A military account can own fleets and open military permit requests (item 18), so letting anyone claim the role was the most serious open item (item 26). Checking the status after the password means the response never tells someone without the password whether an account is pending or suspended, while a real user learns why they cannot log in. Suspension instead of deletion keeps the user's fleets, aircraft and history, which deletion would either block (item 22) or destroy. An authority suspending itself could leave the system without an active authority, and nobody could undo it through the API.

**Known limitations:**

- Access tokens already issued stay valid until they expire (at most 15 minutes) after a user is suspended, because the status is not checked on every request. Closing this gap would need a database lookup per request or a token blacklist.
- Status changes are not recorded (who approved or suspended a user, when, and why), unlike aircraft status changes (item 20).
- Suspending a fleet owner or military commander does not affect their fleets and aircraft.

## 28. Logout

**Date:** 2026-11-16

**Decision:** `POST /api/auth/logout` takes a refresh token in the body (`RefreshRequest`) and does not require an access token. If the token exists, is not revoked and has not expired, it is revoked. The endpoint always returns 204, whether or not anything was revoked. Only that one session ends; other sessions of the user stay valid. Presenting an already revoked token here does not trigger reuse detection (item 12).

**Reason:** The access token may already have expired when the user logs out, so logout must work with the refresh token alone. Always returning 204 means the endpoint reveals nothing about the token and is safe to retry. Reuse detection is left to the refresh endpoint: a client that logs out right after a refresh whose response was lost would otherwise end every session of the user only for trying to log out.

**Known limitation:** The access token of the session stays valid until it expires (at most 15 minutes); the client must discard it.

## 29. Password change

**Date:** 2026-11-16

**Decision:** `POST /api/auth/change-password` requires an access token and takes `currentPassword` and `newPassword`. The new password follows the same rules as registration (8 to 100 characters, shared through `PasswordRules`) and must differ from the current one. A wrong current password returns 400 with a field error on `CurrentPassword`. On success the new hash is stored and every active refresh token of the user is revoked in the same save, including the session that made the change; the endpoint returns 204 and the client must log in again to get a new refresh token.

**Reason:** A wrong current password is invalid input from an authenticated user, not a failed authentication; a 401 would make clients treat their access token as broken and try to refresh. Someone who changes their password usually suspects that it, or a session, is compromised, so every session must end (item 16).

**Known limitation:** Access tokens already issued, including ones held by an attacker, stay valid until they expire (at most 15 minutes, `Jwt:AccessTokenMinutes`). Closing this gap would need a per-user security stamp in the token, checked against the database on every request.













## Retrospective notes

### Project boundaries vs. separation of concerns

**Date:** 2026-11-12

Separating request shape validation from business rules was worth it: validators stay free of the database, and business checks (uniqueness, ownership, allowed transitions) stay in the operations.

Putting validation and DTOs into separate projects was a different decision, and its cost showed up quickly:

- The Dto project cannot see the enums in Dal, so enum values travel as strings and are validated and parsed again (item 24).
- The Validation project needed a reference to Dal to check those same enum names.
- Following a single request means jumping across six projects, which made the code harder to read than the rules it implements.

In a project of my own, I would keep validators, DTOs and repositories as folders in fewer projects, and split a project out only when there is a concrete reason: separate deployment, separate ownership, or a boundary the compiler should enforce. Patterns such as interfaces, adapters and the unit of work earned their place here because each solved a problem we actually hit; the number of projects did not.