# Changelog

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

