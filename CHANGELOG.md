# Changelog

## [v1.0.2] - 2026-07-01
### Added
- Implemented `LanguageIds` in `CompleteProfileDTO` to allow users to select languages during profile completion.
- Added `LanguagesExistAsync`, `RemoveUserLanguagesAsync`, and `AddUserLanguagesAsync` methods to `IUserRepository` and `UserRepository`.

### Changed
- Refactored `UserService.CompleteProfileAsync` for improved EF Core performance.
- Consolidated `LanguageIds` processing to use LINQ `Distinct()` locally and bulk EF methods (`AddRangeAsync`, `ExecuteDeleteAsync`).
- Optimized `UserSkill` insertions to collect skills in a list and insert them in bulk, eliminating repetitive `foreach` loops.
- Updated `UserRepository` to leverage `IReadOnlyCollection<int>` for better query performance and memory footprint.
