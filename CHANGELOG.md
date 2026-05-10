# Changelog

## 1.6.3 — 2026-05-08

### CI/CD

- Added PR validation pipeline (`pipelines/n2-core-ci-pr.yml`) — builds and tests every PR branch; does not publish.
- Removed the PR trigger from the publish pipeline so PR runs and trunk runs no longer overlap.
- Configured build-run retention leases via the Azure DevOps REST API (3 days for PR runs, 10 days for CI runs, 365 days for published runs).

### Fixes

- Removed code complexity in `BackgroundWorker`.
- Fixed flaky `StopJobHonoursCancellationToken` test.
- Fixed race condition in `IsAssignableTo` tests.

## 1.6.1 — 2026-04-13

- **Security:** `Contract.NotNull` is now unconditional — guards no longer stripped in Release builds.
- **Security:** `BackgroundWorker.IsActivated` changed from `static` to instance field — multiple workers no longer share activation state.
- **Security:** JWT validation in `GetPrincipalFromJwt` and `GetPrincipal` now accepts optional `issuer` and `audience` parameters; when supplied, the token is validated against both values.
- **Security:** Basic auth credential parsing in `OAuthCommandHandler` hardened — uses `IndexOf(':')` with explicit guards; passwords containing colons are now handled correctly.
- **Security:** TOTP time window parameter renamed from minutes to seconds across `ValidateTOTP` and `TOTP`; window is now driven by `OAuthConfig.ReplayWindowInSeconds` (default 30 s) rather than a hard-coded value.
- **Security:** `CheckHash` replaced with a constant-time XOR-accumulation comparison; the 500 ms timing floor is now applied once per `ValidateTOTP` call instead of per hash check.
- Added XML documentation to `Validator`, `OAuthConfig`, and `JwtTools`.
- Added `ReplayWindowInSeconds` property to `OAuthConfig` (configurable via `OAuthConfig:ReplayWindowInSeconds`).
- Added unit tests: `WithValidator`, `WithBackgroundWorker`, `WithAzureFunctionClient`, `UsingWaitForObject`, `WithOAuthCommandHandler`; expanded `JwtToolsTests` and `StringExtensionTests`.

## 1.6.0 — 2026-04-12

- Fixed threading issues in `BaseCommandHandler`, `Conductor`, and `Semaphore`.
- Fixed race condition in `DefaultValueService`.
- Stability improvements in `OAuthCommandHandler` under concurrent load.

## 1.5.2 — 2025-04-18

- Added `net10.0` as a target framework.
- All `Microsoft.Extensions.*` packages upgraded to 10.0.5.
- `System.IdentityModel.Tokens.Jwt` upgraded to 8.17.0.
- `RandomStringGenerator` uses `RandomNumberGenerator.GetInt32` on `net9.0`+ for cryptographically secure output.
- `N2.Core.Abstractions` dependency updated to 1.5.2.

## 1.3.0 — 2025-04-15

### Commands

- Added `Conductor` — command dispatcher routing `IRequest` to `IHandle<TRequest, TResponse>` handlers via DI.
- Added `Semaphore` for thread-pool slot management used by the conductor.
- Added `BaseCommandHandler` with async request execution and structured error handling.

### Extensions

- Added `LoggerExtensions` with `LoggerMessage.Define`-based structured logging helpers.
- Extended `StringExtensions`: `UppercaseFirst`, `Truncate`, `GetStableHashCode`, `SanitizeFileName`, `ReplaceChars`, `FindHtmlPart`.

### Other

- Added `Contract` class for unconditional argument validation.
- Added localized resource files (`Messages.resx`, `Messages.nl.resx`).
- Added `net8.0` and `net9.0` as target frameworks alongside `netstandard2.0` / `netstandard2.1`.
- Entity framework support moved to the separate `N2.Core.Entity` package.
- Added unit tests for `Contract`, `Conductor`, `Semaphore`, `HttpResult`.

## 1.0.3 — 2024-08-06

- Adding entity framework support.
- Adding `ChangeLog` as a sample dataset for tracking changes in the application.
- Core functionality for handling change logs, change tracking, and change history.
- Adding `CoreDesignComponent` for easy implementing view-to-entity CRUD operations.
- Updated package references to latest versions.

## 1.0.2 — 2024-05-07

- Using updated `N2.Core.Abstractions` package.
- Improved caching mechanism using `IMemoryCache`.
- Updated `LocalizedTextService` to use `ITextService` for text retrieval.
- Adding OAuth authorization support.
- Adding basic command handler.

## 1.0.0 — 2024-03-29

Initial release of the N2.Core library.
