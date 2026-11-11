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
- Refresh checks, in this order: the token exists, it is not revoked (otherwise item 12), it has not expired, the user still exists. All failures return the same message.
- Revocation sets `RevokedAt`; rows are never deleted on revocation. Deleting a user deletes their refresh tokens (cascade).

**Reason:** Short access tokens limit the damage of a stolen token but cannot be revoked; refresh tokens are stored and can be. A random 512-bit value cannot be guessed, so a fast unsalted hash is enough and allows a direct lookup by hash, which a salted hash would not. The absolute limit stops a stolen token chain from living forever.

**Known limitation:** A session abandoned by the user (for example, after logging in again on the same device) stays valid until it expires. If such a token was stolen, reuse detection never triggers because the real user never presents it again. Revoking all sessions on password change would close this gap and is not implemented yet.