# Changelog

## [v2.0.0] - 2026-09-24

### Added

- Added a dedicated **SkillifyAPI.Application** project for application logic, use cases, DTOs, interfaces, validators, and service abstractions.
- Added a dedicated **SkillifyAPI.Domain** project for core domain entities and business rules.
- Added a dedicated **SkillifyAPI.Infrastructure** project for database access, repositories, external service implementations, and infrastructure concerns.
- Established explicit project dependency boundaries following **Clean Architecture** principles.
- Added solution-level organization to support the new multi-project architecture.

### Changed

- **Major architectural refactor:** migrated the backend from the previous single-project structure to a multi-project **Clean Architecture** solution.
- Separated **API / Presentation**, **Application**, **Domain**, and **Infrastructure** responsibilities into independent projects.
- Moved application-specific logic out of the API project into the **Application** layer.
- Moved domain entities and core business rules into the **Domain** layer.
- Moved infrastructure implementations such as **EF Core**, repositories, and external service integrations into the **Infrastructure** layer.
- Refactored project dependencies to follow **Dependency Inversion**, keeping the Domain layer independent from infrastructure and framework-specific implementations.
- Reorganized existing folders, services, repositories, DTOs, validators, and supporting components according to their architectural responsibilities.
- Changed the Git repository root from the `SkillifyAPI` project directory to the `DEPI` solution directory so the complete multi-project solution is tracked in a single repository.
- Updated the solution structure to support future test projects and additional application components.
- Updated `.gitignore` to exclude Visual Studio **PublishProfiles** and user-specific publish configuration files from source control.

### Refactored

- Refactored the existing backend functionality to work across the new Clean Architecture project boundaries without changing the core platform functionality.
- Decoupled business/application logic from API-specific concerns.
- Decoupled infrastructure implementations from the core application and domain layers.
- Reorganized dependency injection registrations to accommodate the new project structure.
- Reorganized namespaces and project references following the new architectural boundaries.

### Architecture

The solution is now organized as:

- **SkillifyAPI** — Presentation / HTTP API layer.
- **SkillifyAPI.Application** — Application logic, use cases, contracts, DTOs, validators, and abstractions.
- **SkillifyAPI.Domain** — Domain entities and core business rules.
- **SkillifyAPI.Infrastructure** — EF Core, repositories, external services, and infrastructure implementations.

The new dependency structure follows the principle that the **Domain and Application layers remain independent from infrastructure implementation details**, providing a cleaner foundation for future development and testing.

## [v1.3.0] - 2026-07-28

### Changed
- **Platform upgrade:** migrated the project from **.NET 9** to **.NET 10** (`net10.0` target framework).
- **Database upgrade:** migrated from **SQL Server 2019** to **SQL Server 2025** for application data and Hangfire job storage.
- Upgraded core Microsoft packages to **10.0.10**:
  - `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Microsoft.AspNetCore.OpenApi`
- Upgraded **Swashbuckle.AspNetCore** from 6.x to **10.2.3** (with **Microsoft.OpenApi v2**) for .NET 10 compatibility.
- Updated OpenAPI namespace usage from `Microsoft.OpenApi.Models` to `Microsoft.OpenApi` per OpenAPI.NET v2 breaking changes.

### Fixed
- Updated Swagger JWT security configuration in `Program.cs` for Swashbuckle v10: replaced the removed `OpenApiReference` / `OpenApiSecurityScheme.Reference` pattern with `OpenApiSecuritySchemeReference` in `AddSecurityRequirement`, resolving the build error and restoring the Bearer auth button in Swagger UI.

## [v1.2.4] - 2026-07-17

### Added
- Added `PendingRescheduleByUserId` (`int?`) property to `Session` model to track the initiator of a reschedule proposal.
- Added `PendingRescheduleByUser` (`bool`) to `GetSessionDTO` to let the client know if the reschedule was initiated by the current user.
- Added validation check to block the reschedule initiator from sending consecutive reschedule requests without the other participant's response.
- Excluded the current authenticated user from the paginated list returned by `GET /api/users`.

### Changed
- Refactored `SessionValidator.EnsureCanBeRescheduled` and `SessionValidator.EnsureCanAcceptReOffered` to enforce turn-based negotiation using `PendingRescheduleByUserId`.
- Updated `SessionMeetingService` to populate `PendingRescheduleByUser` in `GetSessionDTO` mapping by checking the current user ID context.
- Modified `IUserRepository.GetUsersPagedAsync`, `IUserService.GetUsersAsync`, and `UsersController` to pass down and filter out the current user ID.

## [v1.2.3] - 2026-07-15

### Added
- Enabled both session participants (requester and helper) to submit a rating for the same completed session.
- Added per-user rating state fields to `GetSessionDTO`:
  - `UserRated` — whether the authenticated user has already rated the session.
  - `UserCanRate` — whether the authenticated user is eligible to submit a rating (completed session, not yet rated).
  - `UserRatingScore` — the authenticated user's submitted score when `UserRated` is `true`.
- Exposed the new rating state fields on:
  - `GET /api/Sessions/{sessionId}`
  - `GET /api/Sessions/requested`
  - `GET /api/Sessions/received`
- Added `GetUserRatingForSessionAsync` and `GetUserRatingsForSessionsAsync` to `IRatingRepository` / `RatingRepository` for efficient per-user rating lookups.
- Added EF Core migration `AllowBothUsersToRateSession` — unique index on `(SessionId, ReviewerId)` replaces the previous one-rating-per-session constraint.

### Changed
- Updated `RatingService.SubmitRatingAsync` to allow either participant to rate the other after session completion, with duplicate-rating protection per reviewer.
- Refactored `SessionMeetingService.MapToDto` to populate rating eligibility and score fields based on the calling user's existing rating.
- Updated `AppDbContext` rating configuration to enforce one rating per user per session.

## [v1.1.3] - 2026-07-11

### Added
- Integrated the user's current credit balance alongside transaction history in `GET /api/CreditTransactions/history`.
- Added `CreditTransactionHistoryDto` response payload containing `History` (list of transactions) and `CurrentBalance`.

### Changed
- Updated `ICreditTransactionRepository` and `CreditTransactionRepository` to return both transactions and user's current credit balance as a tuple.
- Refactored `CreditTransactionService` and `CreditTransactionsController` to use the new DTO response contract.
- Synchronized documentation in `README.md` and `QA_BusinessModel.md` (updated to v1.5) to cover current balance response structures, user filtering/auth requirements, and multi-skill/language profile completion parameters.


## [v1.1.2] - 2026-07-06

### Added
- Examples for credit history endpoint return in swagger

### Changed
- Updated `api/users` to get all users and enable filters so now you can search users with name, skills, language, rating



## [v1.0.2] - 2026-07-01
### Added
- Implemented `LanguageIds` in `CompleteProfileDTO` to allow users to select languages during profile completion.
- Added `LanguagesExistAsync`, `RemoveUserLanguagesAsync`, and `AddUserLanguagesAsync` methods to `IUserRepository` and `UserRepository`.

### Changed
- Refactored `UserService.CompleteProfileAsync` for improved EF Core performance.
- Consolidated `LanguageIds` processing to use LINQ `Distinct()` locally and bulk EF methods (`AddRangeAsync`, `ExecuteDeleteAsync`).
- Optimized `UserSkill` insertions to collect skills in a list and insert them in bulk, eliminating repetitive `foreach` loops.
- Updated `UserRepository` to leverage `IReadOnlyCollection<int>` for better query performance and memory footprint.

