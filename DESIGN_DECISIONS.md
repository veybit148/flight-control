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

**Decision:** Every email is trimmed and lowercased with `ToLowerInvariant()` before it is checked for existence, stored, or used to look up a user. Every path that writes or reads emails (registration, login, seed data) must follow this rule.

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