# SkillifyAPI — Comprehensive QA & Software Testing Guide

**Version:** 1.5  
**Prepared For:** QA / Software Testing Team  
**Project:** SkillifyAPI — Skill-exchange platform where users request and offer help sessions.  
**Technology:** ASP.NET Core Web API (.NET 9), Entity Framework Core, FluentValidation, JWT Auth, Hangfire, Cloudinary, ZegoCloud, Firebase Cloud Messaging (FCM)

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Authentication & Security Model](#2-authentication--security-model)
3. [Data Models & Constraints](#3-data-models--constraints)
4. [Validation Rules Reference](#4-validation-rules-reference)
5. [API Endpoint Reference](#5-api-endpoint-reference)
6. [Business Logic & State Machines](#6-business-logic--state-machines)
7. [Credit & Escrow Flow](#7-credit--escrow-flow)
8. [Background Jobs (Hangfire)](#8-background-jobs-hangfire)
9. [Response Contracts (DTOs)](#9-response-contracts-dtos)
10. [HTTP Error Response Catalogue](#10-http-error-response-catalogue)
11. [Detailed Test Case Scenarios](#11-detailed-test-case-scenarios)
12. [Edge Cases & Negative Test Cases](#12-edge-cases--negative-test-cases)

### Document Change Log (v1.5)
| Area | What Changed |
|------|--------------|
| **Profile Completion** | Payload updated to accept multiple `NeededSkills` and `LanguageIds` list. Bulk operations added for performance. |
| **Users Query** | `GET /api/Users` is now **Protected** (`[Authorize]` required) and supports filters: `name`, `skillId`, `minRating`, `langId`. |
| **Credit Transactions** | `GET /api/CreditTransactions/history` response changed to a `CreditTransactionHistoryDto` containing `history` list and `currentBalance`. (v1.4) |
| **Notifications** | Full in-app notification system + Firebase FCM push + device token registration (v1.3) |
| **Credit Transactions (v1.3)** | `GET /api/CreditTransactions/history` + `Description` field on ledger |
| **CreditService** | Centralized credit operations with automatic notifications on every credit change |
| **Daily Gift Job** | Hangfire recurring job gifts 5–100 credits to low-balance users daily |
| **Ratings API** | Full CRUD-read + submit endpoints (`/api/Ratings`) |
| **Security** | JWT `sid` session binding, rate limiting (50 req/min) |
| **Platform** | .NET 9, ZegoCloud (replaced Agora), Swagger v1.3 |

---

## 1. System Overview

**Skillify** is a peer-to-peer skill exchange platform. Users can offer expertise in one skill and receive help for a skill they need. Sessions are virtual meetings scheduled through the API, governed by a credit-based economy, and conducted via ZegoCloud video rooms.

### Core User Roles (Contextual, Not Fixed)
| Role | Description |
|------|-------------|
| **Requester** | The user who initiates or accepts a session request. Pays credits. |
| **Helper** | The user who provides assistance. Receives credits upon session completion. |

> A single user can be both a Requester in one session and a Helper in another simultaneously.

### Core Domain Objects
| Entity | Description |
|--------|-------------|
| `User` | Platform member with a credit wallet |
| `MainSkill` | Top-level skill category (e.g., "Software Engineering") |
| `SubSkill` | Specialization under a MainSkill (e.g., "React", "Node.js") |
| `Language` | Spoken language catalog |
| `Session` | A scheduled 1-on-1 help engagement |
| `SessionEvent` | Audit log entry for every session state change |
| `CreditTransaction` | Immutable ledger record of credit movements |
| `EscrowHold` | Credits locked for a pending/accepted session |
| `Rating` | Post-session review with a decimal score |
| `Badge` | Gamification reward issued to users |
| `Notification` | In-app notification stored per user |
| `UserDevice` | FCM device token for push notification delivery |
| `RefreshToken` | JWT refresh token with rotation and revocation support |
| `PushToken` | Legacy device token model (superseded by `UserDevice` for FCM) |

---

## 2. Authentication & Security Model

### JWT Strategy
- **Access Token**: Short-lived (default: **15 minutes**). Sent as `Bearer` token in the `Authorization` header.
- **Refresh Token**: Long-lived (default: **30 days**). Used to rotate and issue a new access token.
- **Token Rotation**: On every `/refresh` call, the old refresh token is revoked (`IsRevoked = true`, `RevokeReason = "Rotated on refresh"`) and a new one is issued.
- **Global Revocation**: `POST /api/Users/revoke` revokes **all** active refresh tokens for the user (all devices).
- **Per-Session Logout**: `POST /api/Users/logout` revokes only the current session's refresh token (identified from the `sid` JWT claim).
- **Access Token Session Binding**: Every protected request validates the `sid` claim in the JWT against the `RefreshTokens` table. If the linked refresh token is revoked or expired, the access token is rejected even if not yet expired.

### Rate Limiting
- **Policy:** Fixed window — **50 requests per minute** per client.
- **Client key:** Authenticated users are keyed by user ID; anonymous requests are keyed by IP address.
- **Response:** `429 Too Many Requests` with JSON body `{ "status": 429, "error": "Too Many Requests", "message": "..." }` and a `Retry-After` header (seconds).

### Protected vs. Public Endpoints
| Visibility | Endpoints |
|---|---|
| **Public (No Auth)** | `POST /register`, `POST /login`, `POST /refresh`, `GET /api/MainSkills/*`, `GET /api/SubSkills/*`, `GET /api/Languages/*`, `GET /api/Badges`, `GET /api/Ratings/user/{userId}` |
| **Protected (`[Authorize]`)** | `POST /revoke`, `POST /logout`, `GET /api/Users/me`, `PUT /api/Users/me/profile`, `GET /api/Users`, All `/api/Sessions/*`, All `/api/Notifications/*`, `GET /api/CreditTransactions/history`, All other `/api/Ratings/*` |

### Email Normalization
- Emails are normalized (lowercased and trimmed) before storage and lookup. This means `User@Example.COM` and `user@example.com` are treated as the same account.

### Password Hashing
- Passwords are hashed using `ASP.NET Core Identity PasswordHasher` (PBKDF2). Plain-text passwords are **never** stored.

---

## 3. Data Models & Constraints

### 3.1 User
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| `Id` | int | No | Auto | PK |
| `FullName` | string | No | — | — |
| `Email` | string | No | — | **UNIQUE INDEX** |
| `PasswordHash` | string | No | — | Bcrypt/PBKDF2 |
| `ProfilePictureUrl` | string | Yes | null | Cloudinary URL |
| `ProfilePicturePublicId` | string | Yes | null | Cloudinary ID |
| `Bio` | string | Yes | null | Max 500 chars |
| `JobTitle` | string | Yes | null | Max 100 chars |
| `CreditBalance` | int | No | **100** | — |
| `Profile

` | bool | No | **false** | — |
| `LastGiftCreditAt` | DateTime? | Yes | null | Set when user receives a daily gift credit |
| `CreatedAt` | DateTime | No | — | — |
| `UpdatedAt` | DateTime | No | — | — |

### 3.2 Session
| Column | Type | Nullable | Default | Notes |
|--------|------|----------|---------|-------|
| `Id` | int | No | Auto | PK |
| `RequesterId` | int | No | — | FK → User |
| `HelperId` | int | No | — | FK → User |
| `MainSkillId` | int | No | — | FK → MainSkill |
| `Topic` | string | No | — | 3–200 chars |
| `ProblemDescription` | string | No | — | 10–2000 chars |
| `DurationMinutes` | enum | No | — | `15`, `30`, or `60` only |
| `CreditCost` | int | No | — | Matches DurationMinutes value |
| `Status` | enum | No | `Pending` | See state machine below |
| `ScheduledAt` | DateTime | No | — | Must be in the future (UTC) |
| `AcceptedAt` | DateTime? | Yes | null | Set when status → Accepted |
| `CompletedAt` | DateTime? | Yes | null | Set when session closes |
| `CreatedAt` | DateTime | No | — | Set on creation |
| `ZegoRoomId` | string? | Yes | null | GUID string, set on Acceptance |
| `HangfireOpenJobId` | string? | Yes | null | Hangfire job reference |
| `HangfireCloseJobId` | string? | Yes | null | Hangfire job reference |

### Session Status Enum (All Valid Values)
```
Pending → Accepted → Active → Completed
         ↘ Declined
         ↘ Cancelled
         ↘ ReOffered → Accepted (loop)
         ↘ Expired
```

### Session Duration Enum (Only 3 Valid Values)
| Enum Value | Minutes | Credit Cost |
|---|---|---|
| `FifteenMin` | 15 | 15 credits |
| `ThirtyMin` | 30 | 30 credits |
| `SixtyMin` | 60 | 60 credits |

### 3.3 CreditTransaction
| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `UserId` | int | FK → User |
| `SessionId` | int? | Optional, links to session |
| `Type` | enum | `EscrowHold`, `EscrowRelease`, `CreditEarned`, `Refund`, `GiftCredit` |
| `Amount` | int | Negative for deductions, positive for additions |
| `Description` | string? | Optional human-readable description |
| `BalanceAfter` | int | User's balance snapshot after this transaction |
| `CreatedAt` | DateTime | Append-only, never modified |

### 3.4 EscrowHold
| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `SessionId` | int | FK → Session (UNIQUE) — one escrow per session |
| `RequesterId` | int | FK → User |
| `CreditsHeld` | int | Amount locked |
| `Status` | enum | `Held`, `Released`, `Refunded` |
| `HeldAt` | DateTime | When escrow was created |
| `ReleasedAt` | DateTime? | When released to helper or refunded |

### 3.5 Rating
| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `SessionId` | int | FK → Session (UNIQUE) — one rating per session |
| `ReviewerId` | int | FK → User (person giving the review) |
| `RevieweeId` | int | FK → User (person receiving the review) |
| `Score` | decimal(3,1) | Range: 1.0 – 5.0 |
| `ReviewText` | string? | Optional free-text |
| `CreatedAt` | DateTime | — |

### 3.6 Badge
| Field | Notes |
|-------|-------|
| `Slug` | **UNIQUE** — used as stable identifier |
| `CriteriaType` | `SessionCount`, `AverageRating`, `ConsistentHelping` |
| `CriteriaThreshold` | Numeric threshold to unlock this badge |

### 3.7 RefreshToken
| Column | Type | Notes |
|--------|------|-------|
| `Token` | string | Opaque, random value |
| `UserId` | int | FK → User |
| `ExpiresAt` | DateTime | Default 30 days after creation |
| `IsRevoked` | bool | `true` once revoked |
| `RevokedAt` | DateTime? | When it was revoked |
| `RevokeReason` | string? | "Rotated on refresh", "User signed out", etc. |
| `ReplacedByToken` | string? | Token string that replaced this one |

### 3.8 UserSkill
| Column | Type | Notes |
|--------|------|-------|
| `UserId` | int | FK → User |
| `CategoryId` | int | FK → MainSkill |
| `Description` | string | Optional description |
| `SkillType` | enum | `Offered` or `Needed` |

### 3.9 Language
| Column | Type | Notes |
|--------|------|-------|
| `Code` | string | **UNIQUE** — ISO 639-1 code (e.g., `en`, `ar`) |

### 3.10 Notification
| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `UserId` | int | FK → User |
| `Title` | string | Notification heading |
| `Message` | string | Notification body |
| `IsRead` | bool | Default `false` |
| `CreatedAt` | DateTime | — |

**Index:** `(UserId, CreatedAt)` for efficient user notification listing.

### 3.11 UserDevice
| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `UserId` | int | FK → User |
| `FcmToken` | string | **UNIQUE** — Firebase Cloud Messaging token |
| `CreatedAt` | DateTime | — |
| `UpdatedAt` | DateTime | — |
| `IsActive` | bool | `false` when device is unregistered |

**Index:** `(UserId, IsActive)` for active device lookup per user.

### 3.12 SessionEvent (additional field)
| Column | Type | Notes |
|--------|------|-------|
| `Comment` | string? | Optional note attached to a session event (e.g. reschedule reason) |

---

## 4. Validation Rules Reference

### 4.1 Register (`POST /api/Users/register`)

| Field | Rule | Error Message |
|-------|------|---------------|
| `FullName` | Required | — |
| `FullName` | MinLength: **2** | — |
| `FullName` | MaxLength: **100** | — |
| `Email` | Required | — |
| `Email` | Must be valid email format | — |
| `Email` | Must be **unique** in the system | `"Email already exists."` |
| `Password` | Required | — |
| `Password` | Regex: min 8 chars, ≥1 letter, ≥1 digit, ≥1 special char | `"Password must be at least 8 characters and include a letter, a number, and a special character."` |
| `ConfirmPassword` | Required | — |
| `ConfirmPassword` | Must exactly **equal** `Password` | `"Confirm password must match password."` |

**Special characters accepted:** `! @ # $ % ^ & * ( ) _ - + = [ ] { } ; : ' " , . < > / ? \ | ` ~ `

**Regex pattern (for QA reference):**
```
^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'",.<>/?\\|`~]).{8,}$
```

**FullName trimming:** Whitespace is trimmed before storage.

### 4.2 Login (`POST /api/Users/login`)

| Field | Rule |
|-------|------|
| `Email` | Required, valid email format |
| `Password` | Required |

> Emails are normalized to lowercase before lookup. Wrong credentials return `401 Unauthorized` with `{"message": "Invalid credentials."}`.

### 4.3 Token Refresh (`POST /api/Users/refresh`)

| Field | Rule | Error |
|-------|------|-------|
| `RefreshToken` | Must exist in database | `"Invalid refresh token."` |
| `RefreshToken` | Must not be revoked | `"Refresh token revoked."` |
| `RefreshToken` | Must not be expired | `"Refresh token expired."` |

### 4.4 Complete Profile (`PUT /api/Users/me/profile`)

| Field | Rule |
|-------|------|
| `Bio` | Optional, MaxLength: **500** |
| `JobTitle` | Optional, MaxLength: **100** |
| `OfferedDescription` | Optional, MaxLength: **1000** |
| `OfferedMainSkill` | Required, must be an integer **> 0**, must **exist** in the DB |
| `OfferedSubSkills` | Required, array must **not be empty**, all sub-skill IDs must **belong to** `OfferedMainSkill` |
| `NeededSkills` | Required, array must **not be empty**, duplicate main skills are not allowed |
| `NeededSkills[].MainSkillId` | Required, must be an integer **> 0**, must **exist** in the DB |
| `NeededSkills[].SubSkillIds` | Required, array must **not be empty**, all sub-skill IDs must **belong to** `MainSkillId` |
| `NeededSkills[].Description` | Optional, MaxLength: **1000** |
| `LanguageIds` | Optional, list of language IDs, all must **exist** in the DB |
| `ProfilePicture` | Optional, `multipart/form-data` file upload, uploaded to Cloudinary |

> **Important:** When profile is updated, **all previous UserSkills for that user are deleted and replaced** with the new selection. The `ProfileCompleted` flag is set to `true` after the first successful update.

### 4.5 Request Help Session (`POST /api/Sessions/request`)

| Field | Rule | Error |
|-------|------|-------|
| `HelperId` | Required, must exist in DB | `"The requested helper user does not exist."` |
| `HelperId` | Must NOT equal `requesterId` (current user) | `"You cannot request a session with yourself."` |
| `MainSkillId` | Required, must exist in DB | `"The specified skill does not exist."` |
| `Topic` | Required, Length: **3–200 chars** | — |
| `ProblemDescription` | Required, Length: **10–2000 chars** | — |
| `DurationMinutes` | Required, must be one of: `FifteenMin(15)`, `ThirtyMin(30)`, `SixtyMin(60)` | `"Duration must be 15, 30, or 60 minutes."` |
| `ScheduledAt` | Required, must be **in the future** (UTC) | `"Session schedule time must be in the future (UTC)."` |
| Requester's `CreditBalance` | Must be `>= creditCost` (= DurationMinutes value) | `"Insufficient credits. Required: X, Available: Y."` |

### 4.6 Offer Help Session (`POST /api/Sessions/offer`)

| Field | Rule | Error |
|-------|------|-------|
| `RequesterId` | Required, must exist in DB | `"The requested recipient does not exist."` |
| `RequesterId` | Must NOT equal `helperId` (current user) | `"You cannot offer a session to yourself."` |
| `MainSkillId` | Required, must exist in DB | `"The specified skill does not exist."` |
| `Topic` | Required, Length: **3–200 chars** | — |
| `ProblemDescription` | Required, Length: **10–2000 chars** | — |
| `DurationMinutes` | Required, must be one of: `15`, `30`, `60` | — |
| `ScheduledAt` | Required, must be in the future (UTC) | `"Session schedule time must be in the future (UTC)."` |

> **Key Difference from Request Help:** Credits are **NOT deducted** when an offer is created. Credits are only deducted when the **Requester accepts** the offer.

### 4.7 Reschedule Session (`POST /api/Sessions/{sessionId}/reschedule`)

| Field | Rule | Error |
|-------|------|-------|
| `NewScheduledAt` | Required | — |
| `NewScheduledAt` | Must be in the future (UTC) | `"Reschedule time must be in the future (UTC)."` |
| Session Status | Must be `Pending`, `Accepted`, or `ReOffered` | `"You can only reschedule pending, accepted, or re-offered sessions."` |
| User | Must be a participant (Requester or Helper) | `"You are not authorized to reschedule this session."` |

### 4.8 Pagination & Filtering (`GET /api/Users`)

| Parameter | Default | Rule |
|-----------|---------|------|
| `page` | 1 | Must be **>= 1** |
| `pageSize` | 20 | Must be **>= 1** |
| `name` | null | Optional string to filter by user's full name (contains search) |
| `skillId` | null | Optional main skill ID filter (user must have selected this skill) |
| `minRating` | null | Optional decimal to filter users with average rating >= this value |
| `langId` | null | Optional language ID filter (user must know this language) |

### 4.9 Submit Rating (`POST /api/Ratings`)

| Field | Rule | Error Message |
|-------|------|---------------|
| `SessionId` | Required, must be **> 0** | — |
| `Score` | Required, range **1.0 – 5.0** | — |
| `Score` | At most **one decimal place** | `"Score must have at most one decimal place."` |
| `ReviewText` | Optional, MaxLength: **2000** | — |

**Business rules (enforced in service, not FluentValidation):**
| Rule | Error |
|------|-------|
| Session must exist | `"Session not found."` → `404` |
| Caller must be a session participant | `"You are not a participant in this session."` → `401` |
| Session must be `Completed` | `"You can only rate a completed session."` → `400` |
| No existing rating for session | `"This session has already been rated."` → `400` |

> Reviewer = authenticated user. Reviewee = the other session participant.

### 4.10 Register / Unregister Device (`POST/DELETE /api/Notifications/devices`)

| Field | Rule | Error |
|-------|------|-------|
| `FcmToken` | Required, non-empty after trim | `"FCM token is required."` → `400` |

> On unregister, if the token belongs to a different user: `"You are not authorized to unregister this device."` → `401`

### 4.11 Gift Credits (Service-only — no public API controller)

Validators exist for future admin use. Not exposed via HTTP endpoints currently.

**`GiveGiftCreditsDto`:**
| Field | Rule |
|-------|------|
| `UserId` | Must be **> 0** |
| `Amount` | **1 – 1000** (inclusive) |

**`BulkGiftCreditsDto`:**
| Field | Rule |
|-------|------|
| `UserIds` | Required, non-empty array |
| Each `UserId` | Must be **> 0** |
| `Amount` | **1 – 1000** (inclusive) |

---

## 5. API Endpoint Reference

### 5.1 Users Controller (`/api/Users`)

| Method | Endpoint | Auth | Summary |
|--------|----------|------|---------|
| `POST` | `/api/Users/register` | ❌ Public | Register new user, returns JWT tokens |
| `POST` | `/api/Users/login` | ❌ Public | Authenticate user, returns JWT tokens |
| `POST` | `/api/Users/refresh` | ❌ Public | Rotate refresh token, returns new tokens |
| `POST` | `/api/Users/revoke` | ✅ Bearer | Revoke ALL refresh tokens (sign out all devices) |
| `POST` | `/api/Users/logout` | ✅ Bearer | Revoke current-session refresh token only |
| `GET` | `/api/Users/me` | ✅ Bearer | Get authenticated user's full profile |
| `PUT` | `/api/Users/me/profile` | ✅ Bearer | Update profile (multipart/form-data) |
| `GET` | `/api/Users` | ✅ Bearer | Get paginated list of users (filtered by name, skillId, minRating, langId) |

### 5.2 Sessions Controller (`/api/Sessions`)

> **All session endpoints require `Authorization: Bearer <token>`**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `POST` | `/api/Sessions/request` | Request a help session from a specific helper |
| `POST` | `/api/Sessions/offer` | Offer to help a specific requester |
| `POST` | `/api/Sessions/{sessionId}/accept` | Accept a pending/re-offered session |
| `POST` | `/api/Sessions/{sessionId}/decline` | Decline a pending session |
| `POST` | `/api/Sessions/{sessionId}/cancel` | Cancel a pending/accepted/re-offered session |
| `POST` | `/api/Sessions/{sessionId}/reschedule` | Propose a new schedule |
| `GET` | `/api/Sessions/requested` | Get all sessions the current user requested |
| `GET` | `/api/Sessions/received` | Get all sessions the current user received |
| `GET` | `/api/Sessions/{sessionId}` | Get session details (participants only) |
| `GET` | `/api/Sessions/{sessionId}/zego-token` | Get ZegoCloud video room token |

### 5.3 Main Skills Controller (`/api/MainSkills`)

> **All endpoints are public (no auth required)**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/MainSkills` | Get all main skills (flat list, alphabetical) |
| `GET` | `/api/MainSkills/with-subskills` | Get all main skills with their sub-skills |
| `GET` | `/api/MainSkills/{id:int}` | Get a specific main skill with sub-skills by ID |
| `GET` | `/api/MainSkills/slug/{slug}` | Get a specific main skill by slug |

### 5.4 Sub-Skills Controller (`/api/SubSkills`)

> **All endpoints are public**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/SubSkills` | Get all sub-skills (flat list, alphabetical) |
| `GET` | `/api/SubSkills/mainskill/{mainSkillId:int}` | Get sub-skills by parent main skill ID |
| `GET` | `/api/SubSkills/{id:int}` | Get a specific sub-skill by ID |

### 5.5 Languages Controller (`/api/Languages`)

> **All endpoints are public**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/Languages` | Get all languages (alphabetical) |
| `GET` | `/api/Languages/{id:int}` | Get a language by ID |
| `GET` | `/api/Languages/code/{code}` | Get a language by ISO code (e.g., `en`, `ar`) |

### 5.6 Badges Controller (`/api/Badges`)

> **All endpoints are public**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/Badges` | Get all badges in the system |

### 5.7 Notifications Controller (`/api/Notifications`)

> **All endpoints require `Authorization: Bearer <token>`**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/Notifications` | Get all notifications for the current user (newest first) |
| `GET` | `/api/Notifications/unread-count` | Get count of unread notifications |
| `PUT` | `/api/Notifications/{id}/read` | Mark a specific notification as read |
| `PUT` | `/api/Notifications/read-all` | Mark all notifications as read |
| `POST` | `/api/Notifications/devices` | Register or reactivate an FCM device token |
| `DELETE` | `/api/Notifications/devices` | Deactivate an FCM device token |

> When credits change (escrow, refund, release, gift), the system creates an in-app notification and attempts a Firebase push to all active devices for that user. Push delivery failure does **not** roll back the stored notification.

### 5.8 Credit Transactions Controller (`/api/CreditTransactions`)

> **Requires `Authorization: Bearer <token>`**

| Method | Endpoint | Summary |
|--------|----------|---------|
| `GET` | `/api/CreditTransactions/history` | Get the authenticated user's full credit transaction ledger |

### 5.9 Ratings Controller (`/api/Ratings`)

| Method | Endpoint | Auth | Summary |
|--------|----------|------|---------|
| `POST` | `/api/Ratings` | ✅ Bearer | Submit a rating for a completed session |
| `GET` | `/api/Ratings/received` | ✅ Bearer | Get all reviews received by the current user |
| `GET` | `/api/Ratings/given` | ✅ Bearer | Get all reviews submitted by the current user |
| `GET` | `/api/Ratings/user/{userId}` | ❌ Public | Get all public reviews received by a user |
| `GET` | `/api/Ratings/session/{sessionId}` | ✅ Bearer | Get rating for a session (`204 No Content` if none) |

---

## 6. Business Logic & State Machines

### 6.1 Session Lifecycle State Machine

```
                    ┌───────────────────────────────────┐
                    │           SESSION CREATED          │
                    │         Status: Pending            │
                    └──────┬──────────────────┬──────────┘
                           │                  │
              [Helper Accepts]          [Helper Declines]
              (Request flow)            
              [Requester Accepts]
              (Offer flow)
                           │                  │
                    ┌──────▼──────┐   ┌───────▼───────┐
                    │  Accepted   │   │   Declined    │
                    │(ZegoRoom    │   │(Credits       │
                    │ Created)    │   │ Refunded)     │
                    └──────┬──────┘   └───────────────┘
                           │
              [Hangfire fires at ScheduledAt]
                           │
                    ┌──────▼──────┐
                    │   Active    │◄───── (Can cancel from Pending/Accepted)
                    │(ZegoRoom    │                │
                    │  Open)      │         ┌──────▼──────┐
                    └──────┬──────┘         │  Cancelled  │
                           │                │(Credits     │
              [Session duration ends]       │ Refunded)   │
              [Hangfire fires CloseSession] └─────────────┘
                           │
                    ┌──────▼──────┐
                    │  Completed  │
                    │(Credits     │
                    │ Released    │
                    │ to Helper)  │
                    └─────────────┘
                           
        Any status can also have → ReOffered (via /reschedule)
        ReOffered → Accepted or Cancelled
```

### 6.2 Who Can Perform Each Action

| Action | Who Can Do It | Condition |
|--------|---------------|-----------|
| Accept (Request flow) | **Helper only** | Session has an existing EscrowHold (Requester paid) |
| Accept (Offer flow) | **Requester only** | No EscrowHold yet (credits charged on acceptance) |
| Decline | **Helper only** | Session must be `Pending` |
| Cancel | **Either participant** | Session must be `Pending`, `Accepted`, or `ReOffered` |
| Reschedule | **Either participant** | Session must be `Pending`, `Accepted`, or `ReOffered` |
| Accept (ReOffered) | **The OTHER party** who did NOT make the last reschedule | Cannot accept your own reschedule proposal |
| View Session Details | **Either participant** | — |
| Get Zego Token | **Either participant** | Session must be `Accepted` or `Active`, within time window |
| Submit Rating | **Either participant** | Session must be `Completed`; no existing rating for session |
| View Session Rating | **Either participant** | — |
| View User's Public Reviews | **Anyone** | `GET /api/Ratings/user/{userId}` |

### 6.3 ZegoCloud Token Access Rules

| Rule | Condition | HTTP Status |
|------|-----------|-------------|
| Must be a participant | `RequesterId == userId OR HelperId == userId` | `403 Forbidden` if fails |
| Session must be started | `now >= scheduledAt - 2 minutes` | `400` `"Session hasn't started yet."` |
| Session must not be over | `now <= scheduledAt + durationMinutes` | `400` `"Session has already ended."` |
| Session must be live | Status must be `Accepted` or `Active` | `400` `"Session is not available to join."` |
| Room must be configured | `ZegoRoomId` must not be null/empty | `500` `"Room not configured."` |

**Important:** Access is allowed from **2 minutes before** start time (early-join window).

---

## 7. Credit & Escrow Flow

### 7.1 Initial Balance
- Every new user starts with **100 credits** automatically.

### 7.1.1 CreditService (Centralized Credit Operations)
All credit movements are handled by `CreditService`:
- `DeductCreditsAsync()` — escrow holds
- `AddCreditsAsync()` — escrow release, refunds, gifts
- `GiveCreditsAsync()` — single-user manual gift (service-only, no HTTP endpoint)
- `GiveCreditsToUsersAsync()` — bulk gift (used by daily gift job)

Each credit change creates a `CreditTransaction` ledger entry and triggers an in-app notification (with optional Firebase push).

### 7.2 Flow A: Requester Initiates Help Request
```
1. Requester calls POST /api/Sessions/request
2. System validates Requester has enough credits (>= session cost)
3. Credits IMMEDIATELY deducted: Requester.CreditBalance -= creditCost
4. EscrowHold created: { Status = Held, CreditsHeld = creditCost }
5. CreditTransaction created: { Type = EscrowHold, Amount = -creditCost }
6. Session created with Status = Pending
─────────────────────────────────────────────
ON ACCEPT (Helper accepts):
   No additional credit movement (already escrowed)
   ZegoRoomId and Hangfire jobs scheduled
─────────────────────────────────────────────
ON DECLINE / CANCEL:
   EscrowHold.Status = Refunded
   Requester.CreditBalance += CreditsHeld
   CreditTransaction: { Type = Refund, Amount = +creditCost }
   In-app notification + push: "Credits Refunded — You have been refunded X credits."
─────────────────────────────────────────────
ON COMPLETION (Hangfire CloseSession):
   EscrowHold.Status = Released
   Helper.CreditBalance += CreditsHeld
   CreditTransaction: { Type = EscrowRelease, Amount = +creditCost, UserId = HelperId }
   In-app notification + push: "Credits Earned — You earned X credits from completing a session."
```

### 7.3 Flow B: Helper Initiates Help Offer
```
1. Helper calls POST /api/Sessions/offer
2. Session created with Status = Pending
3. NO credits deducted yet
─────────────────────────────────────────────
ON ACCEPT (Requester accepts):
   System validates Requester has enough credits
   Credits deducted: Requester.CreditBalance -= creditCost
   EscrowHold created: { Status = Held }
   CreditTransaction: { Type = EscrowHold }
   ZegoRoomId and Hangfire jobs scheduled
─────────────────────────────────────────────
ON DECLINE / CANCEL:
   Refund if EscrowHold exists with Status = Held
ON COMPLETION:
   Same as Flow A
```

### 7.4 Credit Transaction Types Summary

| Type | When | Amount Sign | Affected User |
|------|------|-------------|---------------|
| `EscrowHold` | Session requested / offer accepted | **Negative** | Requester |
| `Refund` | Session declined or cancelled | **Positive** | Requester |
| `EscrowRelease` | Session completed | **Positive** | Helper |
| `CreditEarned` | System-generated reward (e.g. badge) | **Positive** | Any user |
| `GiftCredit` | Daily gift job or manual gift | **Positive** | Any user |

### 7.5 Daily Gift Credit Flow (Background Job)
```
1. Hangfire job "daily-gift-credits" runs daily at 03:00 UTC
2. System finds eligible users:
   - CreditBalance < 15
   - LastGiftCreditAt is null OR more than 30 days ago
3. Each eligible user receives a random amount between 5 and 100 credits
4. User.LastGiftCreditAt is set to DateTime.UtcNow
5. CreditTransaction created: { Type = GiftCredit, Amount = +giftAmount }
6. In-app notification + push: "Gift Credits — You received X credits."
```

> Manual single/bulk gift via `CreditService.GiveCreditsAsync()` is implemented but **not exposed** via a public API controller.

---

## 8. Background Jobs (Hangfire)

The system uses **Hangfire** for time-based session lifecycle automation. Jobs have **3 automatic retry attempts**.

| Job | Trigger | Action |
|-----|---------|--------|
| `OpenSession(sessionId)` | Fires at `session.ScheduledAt` (UTC) | Changes status `Accepted → Active`, schedules `CloseSession` |
| `CloseSession(sessionId)` | Fires at `scheduledAt + durationMinutes` | Changes status `Active → Completed`, closes Zego room, releases escrow to helper |
| `DailyGift.ExecuteAsync()` | **Daily at 03:00 UTC** (recurring) | Gifts random 5–100 credits to eligible low-balance users |

### Job Cancellation
- When a session is **cancelled** while `Accepted`: Both `HangfireOpenJobId` and `HangfireCloseJobId` are deleted via `BackgroundJob.Delete()`.
- When a session is **rescheduled** from `Accepted` status: Jobs are cancelled, `ZegoRoomId` is cleared, `AcceptedAt` is reset to null. The session goes to `ReOffered` status and must be re-accepted to get a new room and new jobs.

### Hangfire Dashboard
- Available at `/hangfire` (development and production). Currently has no authorization filter — restrict access in production deployments.

---

## 9. Response Contracts (DTOs)

### 9.1 Auth Response (`AuthResponseDto`)
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "abc123...",
  "accessTokenExpiresInSeconds": 900,
  "refreshTokenExpiresAt": "2026-07-12T20:00:00Z",
  "accessTokenExpiresAt": "2026-06-12T20:15:00Z"
}
```

### 9.2 User Profile (`GetUserProfileData`)
```json
{
  "userID": 1,
  "fullName": "Ahmed Mohamed",
  "bio": "Software developer with 5 years experience",
  "profilePictureUrl": "https://res.cloudinary.com/...",
  "jobTitle": "Backend Developer",
  "creditBalance": 85,
  "badges": [],
  "languages": [],
  "offeredSkill": {
    "mainSkillId": 1,
    "mainSkillName": "Software Engineering",
    "description": "I can help with Node.js",
    "subSkills": []
  },
  "neededSkill": { ... },
  "completedSessions": "5",
  "receivedReviews": [],
  "overallRatingScore": 4.5
}
```

### 9.3 Session Response (`GetSessionDTO`)
```json
{
  "id": 42,
  "requesterId": 1,
  "requesterName": "Ahmed Mohamed",
  "helperId": 2,
  "helperName": "Sara Ali",
  "mainSkillId": 3,
  "mainSkillName": "Software Engineering",
  "topic": "Help with React hooks",
  "problemDescription": "I need help understanding useEffect...",
  "durationMinutes": 30,
  "creditCost": 30,
  "status": "Accepted",
  "scheduledAt": "2026-06-15T14:00:00Z",
  "acceptedAt": "2026-06-12T18:00:00Z",
  "completedAt": null,
  "createdAt": "2026-06-12T17:00:00Z",
  "zegoRoomId": "a1b2c3d4e5f6..."
}
```

### 9.4 Received Review (`GetReceivedReviewDTO`)
```json
{
  "id": 10,
  "sessionId": 42,
  "score": 4.5,
  "reviewText": "Very helpful and patient!",
  "createdAt": "2026-06-15T15:30:00Z",
  "reviewer": {
    "userId": 2,
    "fullName": "Sara Ali",
    "profilePictureUrl": "..."
  }
}
```

### 9.5 Paginated Response (`PagedResult<T>`)
```json
{
  "items": [ ... ],
  "totalCount": 100
}
```

### 9.6 Notification (`NotificationDto`)
```json
{
  "id": 1,
  "title": "Credits Earned",
  "message": "You earned 30 credits from completing a session.",
  "isRead": false,
  "createdAt": "2026-06-15T15:30:00Z"
}
```

### 9.7 Unread Count (`UnreadCountDto`)
```json
{
  "count": 3
}
```

### 9.8 Credit Transaction History (`CreditTransactionHistoryDto`)
```json
{
  "history": [
    {
      "id": 55,
      "userId": 1,
      "amount": 30,
      "type": "EscrowRelease",
      "description": null,
      "createdAt": "2026-06-15T15:30:00Z"
    }
  ],
  "currentBalance": 100
}
```

### 9.9 Submit Rating Request (`SubmitRatingDTO`)
```json
{
  "sessionId": 42,
  "score": 4.5,
  "reviewText": "Very helpful and patient!"
}
```

### 9.10 Full Rating Response (`GetRatingDTO`)
```json
{
  "id": 10,
  "sessionId": 42,
  "score": 4.5,
  "reviewText": "Very helpful and patient!",
  "createdAt": "2026-06-15T15:30:00Z",
  "reviewer": {
    "userId": 1,
    "fullName": "Ahmed Mohamed",
    "profilePictureUrl": "..."
  },
  "reviewee": {
    "userId": 2,
    "fullName": "Sara Ali",
    "profilePictureUrl": "..."
  }
}
```

### 9.11 Register Device Request (`RegisterDeviceDto`)
```json
{
  "fcmToken": "firebase-device-token-string"
}
```

---

## 10. HTTP Error Response Catalogue

All error responses return a JSON body in this format:
```json
{ "message": "Human-readable error description" }
```

### 10.1 Authentication Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `400` | Duplicate email on register | `"Email already exists."` |
| `401` | Wrong email or password | `"Invalid credentials."` |
| `401` | Refresh token not found | `"Invalid refresh token."` |
| `401` | Refresh token revoked | `"Refresh token revoked."` |
| `401` | Refresh token expired | `"Refresh token expired."` |
| `401` | Access token's linked refresh token revoked (`sid` claim) | JWT middleware rejects request |
| `429` | Rate limit exceeded | `"Rate limit exceeded. Please try again in X seconds."` |
| `500` | Unexpected server error | `"An unexpected error occurred during registration."` |

### 10.2 Session Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `400` | Scheduled time in the past | `"Session schedule time must be in the future (UTC)."` |
| `400` | Invalid duration value | `"Duration must be 15, 30, or 60 minutes."` |
| `400` | Self-request | `"You cannot request a session with yourself."` |
| `400` | Self-offer | `"You cannot offer a session to yourself."` |
| `400` | Insufficient credits | `"Insufficient credits. Required: X, Available: Y."` |
| `400` | Accept non-pending session | `"Only pending or re-offered sessions can be accepted."` |
| `400` | Accept own reschedule | `"You cannot accept your own reschedule proposal."` |
| `400` | Decline non-pending session | `"Only pending sessions can be declined."` |
| `400` | Cancel wrong status | `"Only pending, accepted, or re-offered sessions can be cancelled."` |
| `400` | Reschedule wrong status | `"You can only reschedule pending, accepted, or re-offered sessions."` |
| `401` | Accept/Decline wrong user | `"Only the helper can accept this session request."` |
| `401` | Cancel not a participant | `"You are not authorized to cancel this session."` |
| `401` | View session not participant | `"You are not authorized to view this session."` |
| `403` | Zego token, not a participant | `Forbid()` (no body) |
| `404` | Session not found | `"Session not found."` |
| `404` | Helper not found | `"The requested helper user does not exist."` |
| `404` | Skill not found | `"The specified skill does not exist."` |

### 10.3 Profile Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `400` | Invalid main skill ID | `"Main skill does not exist."` |
| `400` | Sub-skill not in main skill | `"One or more sub-skills are invalid for the selected main skill."` |
| `400` | Invalid pagination params | `"Page and page size must be greater than or equal to 1."` |
| `404` | User not found | `"User not found."` |

### 10.4 Notification Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `400` | Missing FCM token on register/unregister | `"FCM token is required."` |
| `401` | Unregister token belonging to another user | `"You are not authorized to unregister this device."` |
| `404` | Notification not found on mark-as-read | `"Notification not found."` |
| `401` | Mark-as-read on another user's notification | `"You are not authorized to access this notification."` |

### 10.5 Rating Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `400` | Score out of range or too many decimals | FluentValidation message |
| `400` | Session not completed | `"You can only rate a completed session."` |
| `400` | Session already rated | `"This session has already been rated."` |
| `401` | Non-participant submits rating | `"You are not a participant in this session."` |
| `401` | Non-participant views session rating | `"You are not authorized to view this session's rating."` |
| `404` | Session not found | `"Session not found."` |

### 10.6 Rate Limiting Errors
| Status | Scenario | Message |
|--------|----------|---------|
| `429` | More than 50 requests in 1 minute | `"Rate limit exceeded. Please try again in X seconds."` |

---

## 11. Detailed Test Case Scenarios

### TC-AUTH-001: Successful User Registration
- **Input:** `{ fullName: "Ahmed Mohamed", email: "ahmed@test.com", password: "Pass@1234", confirmPassword: "Pass@1234" }`
- **Expected:** `200 OK` with `AuthResponseDto` containing non-null `accessToken` and `refreshToken`.
- **Verify:** `accessTokenExpiresInSeconds` = 900 (15 min), `refreshTokenExpiresAt` ~30 days from now.

### TC-AUTH-002: Registration with Duplicate Email
- **Input:** Same email as a previously registered user.
- **Expected:** `400 Bad Request`, `{ "message": "Email already exists." }`

### TC-AUTH-003: Password Without Special Character
- **Input:** `password: "Password123"` (no special char)
- **Expected:** `400 Bad Request` with validation error message.

### TC-AUTH-004: Password Without Number
- **Input:** `password: "Password@abc"` (no digit)
- **Expected:** `400 Bad Request`

### TC-AUTH-005: ConfirmPassword Mismatch
- **Input:** `password: "Pass@1234"`, `confirmPassword: "Pass@5678"`
- **Expected:** `400 Bad Request`, `{ "message": "Confirm password must match password." }`

### TC-AUTH-006: Token Refresh
- **Pre-condition:** Valid refresh token obtained from login.
- **Expected:** `200 OK` with a NEW set of tokens. Old refresh token is **now invalid**.

### TC-AUTH-007: Use Revoked Refresh Token
- **Pre-condition:** Login, refresh once (old token revoked), try to use old token again.
- **Expected:** `401 Unauthorized`, `{ "message": "Refresh token revoked." }`

### TC-AUTH-008: Logout and Use Old Token
- **Pre-condition:** Login, save refresh token, call `POST /logout`.
- **Action:** Call `POST /refresh` with the old refresh token.
- **Expected:** `401 Unauthorized`

### TC-AUTH-009: Revoke All Devices
- **Pre-condition:** User has 3 devices logged in (3 refresh tokens).
- **Action:** Call `POST /revoke` on any device.
- **Expected:** ALL 3 refresh tokens are now revoked. All devices are signed out.

---

### TC-PROFILE-001: Complete Profile Successfully
- **Pre-condition:** Registered user with `profileCompleted = false`.
- **Input:** Multipart form with valid `OfferedMainSkill`, `OfferedSubSkills`, `NeededMainSkill`, `NeededSubSkills`.
- **Expected:** `200 OK` with full `GetUserProfileData`. `profileCompleted` is `true`.

### TC-PROFILE-002: Sub-skills Not Belonging to Main Skill
- **Input:** `offeredMainSkill = 1`, but `offeredSubSkills = [99, 100]` which belong to MainSkill 5.
- **Expected:** `400 Bad Request`, `{ "message": "One or more sub-skills are invalid for the selected main skill." }`

### TC-PROFILE-003: Profile Picture Upload
- **Input:** Send a `.jpg` file in `profilePicture` field.
- **Expected:** `200 OK`, `profilePictureUrl` is a valid Cloudinary URL.

### TC-PROFILE-004: Update Profile Replaces Skills
- **Pre-condition:** User has OfferedSkill = "Python".
- **Action:** Call `PUT /me/profile` with OfferedSkill = "JavaScript".
- **Expected:** Old "Python" skill is deleted, new "JavaScript" skill exists. No duplicate skill rows.

### TC-PROFILE-005: Bio Exceeds Max Length
- **Input:** `bio` string with 501 characters.
- **Expected:** `400 Bad Request`

---

### TC-SESSION-001: Request Help — Happy Path
- **Pre-condition:** Requester has 100 credits. Helper is a different user. MainSkillId exists.
- **Input:** `{ helperId: X, mainSkillId: Y, topic: "Help me!", problemDescription: "I need help with...", durationMinutes: "ThirtyMin", scheduledAt: "<future UTC>" }`
- **Expected:** `201 Created` with `GetSessionDTO`. Status = `"Pending"`.
- **Verify:** Requester's credit balance is now **70** (100 - 30). An EscrowHold with `creditsHeld = 30` exists. A `CreditTransaction` of type `EscrowHold` with amount `-30` is logged.

### TC-SESSION-002: Request Help — Insufficient Credits
- **Pre-condition:** Requester has 20 credits. Requesting a 30-minute session.
- **Expected:** `400 Bad Request`, `{ "message": "Insufficient credits. Required: 30, Available: 20." }`

### TC-SESSION-003: Request Help — Past Scheduled Time
- **Input:** `scheduledAt` = 1 hour in the past.
- **Expected:** `400 Bad Request`, `{ "message": "Session schedule time must be in the future (UTC)." }`

### TC-SESSION-004: Request Help with Yourself
- **Input:** `helperId` = current user's own ID.
- **Expected:** `400 Bad Request`, `{ "message": "You cannot request a session with yourself." }`

### TC-SESSION-005: Offer Help — Credit Not Deducted Immediately
- **Pre-condition:** Helper calls `POST /offer`.
- **Expected:** `201 Created`. **Helper's credit balance is unchanged.** No EscrowHold created.

### TC-SESSION-006: Offer Help — Requester Accepts (Credit Check)
- **Pre-condition:** Helper offered a 60-minute session. Requester has 60 credits.
- **Action:** Requester calls `POST /{sessionId}/accept`.
- **Expected:** `200 OK`. Requester's balance is now **0**. EscrowHold created with `creditsHeld = 60`.

### TC-SESSION-007: Offer Help — Requester Accepts with Insufficient Credits
- **Pre-condition:** Helper offered a 60-minute session. Requester has only 30 credits.
- **Action:** Requester calls `POST /{sessionId}/accept`.
- **Expected:** `400 Bad Request`, `{ "message": "Insufficient credits from requester. Required: 60, Available: 30." }`

### TC-SESSION-008: Helper Declines — Requester Gets Refund
- **Pre-condition:** Session was requested (credits escrowed). Status = `Pending`.
- **Action:** Helper calls `POST /{sessionId}/decline`.
- **Expected:** `200 OK`. Session status = `"Declined"`. Requester's credits restored. EscrowHold status = `"Refunded"`. A `CreditTransaction` of type `Refund` is created.

### TC-SESSION-009: Helper Cannot Decline Accepted Session
- **Pre-condition:** Session status = `Accepted`.
- **Action:** Helper calls `POST /{sessionId}/decline`.
- **Expected:** `400 Bad Request`, `{ "message": "Only pending sessions can be declined." }`

### TC-SESSION-010: Cancel Accepted Session — Zego Jobs Cancelled
- **Pre-condition:** Session has been accepted. Hangfire jobs are scheduled.
- **Action:** Either participant cancels.
- **Expected:** `200 OK`. Session status = `"Cancelled"`. Credits refunded to Requester. Hangfire jobs deleted.

### TC-SESSION-011: Cancel Completed Session (Negative)
- **Pre-condition:** Session status = `"Completed"`.
- **Expected:** `400 Bad Request`

### TC-SESSION-012: Third Party Cannot View Session
- **Pre-condition:** Session between UserA and UserB.
- **Action:** UserC (not a participant) calls `GET /{sessionId}`.
- **Expected:** `401 Unauthorized`

### TC-SESSION-013: Reschedule Accepted Session — Room Cleared
- **Pre-condition:** Session is `Accepted`. It has a `ZegoRoomId` and Hangfire jobs.
- **Action:** Either participant reschedules.
- **Expected:** `200 OK`. Status = `"ReOffered"`. `ZegoRoomId = null`. `AcceptedAt = null`. Hangfire jobs cancelled.

### TC-SESSION-014: Cannot Accept Own Reschedule
- **Pre-condition:** UserA rescheduled, session is `ReOffered`.
- **Action:** UserA tries to accept.
- **Expected:** `400 Bad Request`, `{ "message": "You cannot accept your own reschedule proposal." }`

### TC-SESSION-015: Zego Token — Early Join (2 Min Before)
- **Pre-condition:** Session is `Accepted`. Current time = `scheduledAt - 1 minute 50 seconds`.
- **Expected:** `200 OK` with token and room details.

### TC-SESSION-016: Zego Token — Too Early (> 2 Min Before)
- **Pre-condition:** Current time = `scheduledAt - 5 minutes`.
- **Expected:** `400 Bad Request`, `{ "message": "Session hasn't started yet." }`

### TC-SESSION-017: Zego Token — After Session Ends
- **Pre-condition:** Current time = `scheduledAt + durationMinutes + 1 second`.
- **Expected:** `400 Bad Request`, `{ "message": "Session has already ended." }`

---

### TC-NOTIF-001: Get My Notifications
- **Pre-condition:** Authenticated user with at least one notification.
- **Expected:** `200 OK` with array of `NotificationDto`, ordered newest first.

### TC-NOTIF-002: Get Unread Count
- **Pre-condition:** User has 3 unread notifications.
- **Expected:** `200 OK`, `{ "count": 3 }`

### TC-NOTIF-003: Mark Notification as Read
- **Pre-condition:** Notification belongs to current user, `isRead = false`.
- **Expected:** `200 OK`, `{ "message": "Notification marked as read." }`

### TC-NOTIF-004: Mark Another User's Notification as Read
- **Expected:** `401 Unauthorized`

### TC-NOTIF-005: Register Device Token
- **Input:** `{ "fcmToken": "valid-token" }`
- **Expected:** `200 OK`, `{ "message": "Device registered successfully." }`

### TC-NOTIF-006: Register Device — Missing Token
- **Input:** `{ "fcmToken": "" }`
- **Expected:** `400 Bad Request`, `{ "message": "FCM token is required." }`

### TC-NOTIF-007: Credit Change Triggers Notification
- **Pre-condition:** Session completes, helper earns credits.
- **Expected:** Helper receives in-app notification with title `"Credits Earned"`.

---

### TC-CREDIT-001: Get Credit History
- **Pre-condition:** User has completed at least one session.
- **Expected:** `200 OK` with `CreditTransactionHistoryDto` containing `history` array (including `EscrowHold`, `EscrowRelease`, or `Refund` entries) and user's `currentBalance`.

### TC-CREDIT-002: Credit History Requires Auth
- **Action:** Call `GET /api/CreditTransactions/history` without token.
- **Expected:** `401 Unauthorized`

---

### TC-RATING-001: Submit Rating — Happy Path
- **Pre-condition:** Session is `Completed`. No existing rating. User is a participant.
- **Input:** `{ "sessionId": 42, "score": 4.5, "reviewText": "Great session!" }`
- **Expected:** `201 Created` with `GetRatingDTO`. Reviewer = current user, reviewee = other participant.

### TC-RATING-002: Submit Rating — Session Not Completed
- **Pre-condition:** Session status = `Accepted`.
- **Expected:** `400 Bad Request`, `{ "message": "You can only rate a completed session." }`

### TC-RATING-003: Submit Rating — Duplicate
- **Pre-condition:** Session already has a rating.
- **Expected:** `400 Bad Request`, `{ "message": "This session has already been rated." }`

### TC-RATING-004: Submit Rating — Non-Participant
- **Action:** UserC (not in session) submits rating.
- **Expected:** `401 Unauthorized`

### TC-RATING-005: Submit Rating — Invalid Score
- **Input:** `{ "sessionId": 42, "score": 6.0 }`
- **Expected:** `400 Bad Request` (FluentValidation)

### TC-RATING-006: Get User Public Reviews
- **Action:** `GET /api/Ratings/user/2` (no auth required).
- **Expected:** `200 OK` with array of `GetReceivedReviewDTO`.

### TC-RATING-007: Get Session Rating — No Rating Yet
- **Pre-condition:** Completed session with no rating. User is participant.
- **Expected:** `204 No Content`

### TC-RATING-008: Profile Shows Overall Rating
- **Pre-condition:** User has received reviews with scores 4.0 and 5.0.
- **Action:** `GET /api/Users/me`
- **Expected:** `overallRatingScore = 4.5`, `receivedReviews` array populated.

---

### TC-GIFT-001: Daily Gift — Eligible User
- **Pre-condition:** User has `CreditBalance = 10`, `LastGiftCreditAt = null`.
- **Action:** Daily gift job runs.
- **Expected:** Balance increases by 5–100. `LastGiftCreditAt` updated. `GiftCredit` transaction created. Notification sent.

### TC-GIFT-002: Daily Gift — Ineligible (Recent Gift)
- **Pre-condition:** User received gift 10 days ago (`LastGiftCreditAt` within 30 days).
- **Expected:** User is skipped.

### TC-GIFT-003: Daily Gift — Ineligible (Balance >= 15)
- **Pre-condition:** User has `CreditBalance = 20`.
- **Expected:** User is skipped.

---

## 12. Edge Cases & Negative Test Cases

### Authentication
| # | Scenario | Expected |
|---|----------|----------|
| E-01 | Access protected endpoint with expired access token | `401 Unauthorized` |
| E-02 | Access protected endpoint with no token | `401 Unauthorized` |
| E-03 | Register with email `USER@EXAMPLE.COM`, login with `user@example.com` | Login should succeed (email normalized) |
| E-04 | Send `FullName` with only whitespace | Validation fails (after trimming, length < 2) |
| E-05 | Send refresh token belonging to a different user | `401 Unauthorized` |
| E-05a | Use access token after logout (linked refresh token revoked via `sid`) | `401 Unauthorized` |

### Sessions
| # | Scenario | Expected |
|---|----------|----------|
| E-06 | Request a session with a non-existent `helperId` | `404 Not Found` |
| E-07 | Request a session with a non-existent `mainSkillId` | `404 Not Found` |
| E-08 | Send `topic` with 2 characters (below minimum) | `400 Bad Request` |
| E-09 | Send `topic` with 201 characters (above maximum) | `400 Bad Request` |
| E-10 | Send `problemDescription` with 9 characters (below minimum) | `400 Bad Request` |
| E-11 | Send `durationMinutes` with value `45` (not in enum) | `400 Bad Request` |
| E-12 | Helper attempts to decline a session they didn't receive | `401 Unauthorized` |
| E-13 | Cancel a session that is `Active` (already in session) | `400 Bad Request` |
| E-14 | Get Zego token for a `Pending` session | `400 Bad Request` |
| E-15 | Get Zego token when `ZegoRoomId` is null (edge condition) | `500 Internal Server Error` |
| E-16 | Reschedule a `Completed` session | `400 Bad Request` |
| E-17 | Paginate users with `page=0` | `400 Bad Request` |
| E-18 | Paginate users with `pageSize=-1` | `400 Bad Request` |
| E-19 | Request 60-min session when balance is exactly 60 (boundary) | `201 Created` — exact match allowed |
| E-20 | Request 60-min session when balance is exactly 59 (boundary) | `400 Bad Request` |

### Skills & Catalog
| # | Scenario | Expected |
|---|----------|----------|
| E-21 | Get main skill by non-existent ID | `404 Not Found` |
| E-22 | Get sub-skills for non-existent `mainSkillId` | `404 Not Found` |
| E-23 | Get language by non-existent code | `404 Not Found` |
| E-24 | Get language by code in uppercase (e.g., `EN`) | Should be case-insensitive lookup — `200 OK` |

### Notifications
| # | Scenario | Expected |
|---|----------|----------|
| E-25 | Access notifications without token | `401 Unauthorized` |
| E-26 | Mark non-existent notification as read | `404 Not Found` |
| E-27 | Register same FCM token for different user | Token reassigned to new user, `IsActive = true` |

### Ratings
| # | Scenario | Expected |
|---|----------|----------|
| E-28 | Rate a `Pending` session | `400 Bad Request` |
| E-29 | Rate with score `3.55` (two decimal places) | `400 Bad Request` |
| E-30 | Third party views session rating | `401 Unauthorized` |
| E-31 | `ReviewText` exceeds 2000 characters | `400 Bad Request` |

### Rate Limiting
| # | Scenario | Expected |
|---|----------|----------|
| E-32 | Send 51 requests in 1 minute from same user | `429 Too Many Requests` on 51st request |

### Credit History
| # | Scenario | Expected |
|---|----------|----------|
| E-33 | Get credit history without auth | `401 Unauthorized` |

---

*Document generated from source code analysis of SkillifyAPI.*  
*Swagger version: v1.3*  
*Controllers: UsersController, SessionsController, MainSkillsController, SubSkillsController, LanguagesController, BadgesController, NotificationsController, CreditTransactionsController, RatingsController*  
*Validators: RegisterValidator, LoginValidator, CompleteProfileValidator, SubmitRatingValidator, GiveGiftCreditsDtoValidator, BulkGiftCreditsDtoValidator*  
*Services: UserService, SessionMeetingService, NotificationService, CreditService, CreditTransactionService, RatingService, BadgeService, MainSkillService, SubSkillService, LanguageService*  
*Background Jobs: OpenSession, CloseSession, DailyGift*
