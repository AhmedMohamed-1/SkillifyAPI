# Changelog

## [v1.1.3] - 2026-07-11

### Added
- Integrated the user's current credit balance alongside transaction history in `GET /api/CreditTransactions/history`.
- Added `CreditTransactionHistoryDto` response payload containing `History` (list of transactions) and `CurrentBalance`.

### Changed
- Updated `ICreditTransactionRepository` and `CreditTransactionRepository` to return both transactions and user's current credit balance as a tuple.
- Refactored `CreditTransactionService` and `CreditTransactionsController` to use the new DTO response contract.
- Synchronized documentation in `README.md` and `QA_BusinessModel.md` (updated to v1.5) to cover current balance response structures, user filtering/auth requirements, and multi-skill/language profile completion parameters.

## [v1.0.2] - 2026-07-01
### Added
- Implemented `LanguageIds` in `CompleteProfileDTO` to allow users to select languages during profile completion.
- Added `LanguagesExistAsync`, `RemoveUserLanguagesAsync`, and `AddUserLanguagesAsync` methods to `IUserRepository` and `UserRepository`.

### Changed
- Refactored `UserService.CompleteProfileAsync` for improved EF Core performance.
- Consolidated `LanguageIds` processing to use LINQ `Distinct()` locally and bulk EF methods (`AddRangeAsync`, `ExecuteDeleteAsync`).
- Optimized `UserSkill` insertions to collect skills in a list and insert them in bulk, eliminating repetitive `foreach` loops.
- Updated `UserRepository` to leverage `IReadOnlyCollection<int>` for better query performance and memory footprint.

## [v1.1.2] - 2026-07-06

### Added
- Examples for credit history endpoint return in swagger

### Changed
- Updated `api/users` to get all users and enable filters so now you can search users with name, skills, language, rating