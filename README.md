[![.NET Build and test](https://github.com/Kaalenco/n2-core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Kaalenco/n2-core/actions/workflows/dotnet.yml)

# n2-core
Basic functionality for any project

## Change Log

### 1.0.0
- Initial release of the N2.Core library.

### 1.0.2

- Using updated `N2.Core.Abstraction` package
- Improved caching mechanism using `IMemoryCache`.
- Updated `LocalizedTextService` to use `ITextService` for text retrieval.
- Adding OAuth authorization support.
- Adding basic command handler
- Update to .NET 9.0

### 1.0.3

- Adding entity framework support
- Adding ChangeLog as a sample dataset for tracking changes in the application.
- Core functionality for handling change logs, change tracking, and change history.
- Adding CoreDesignComponent for easy implementing view to entity CRUD operations.
- Update package references to latest versions.

### 1.3.0

- Added `net8.0` and `net9.0` as target frameworks alongside `netstandard2.0` / `netstandard2.1`.
- Entity framework support moved to the separate `N2.Core.Entity` package.
- Added `Conductor` — command dispatcher routing `IRequest` to `IHandle<TRequest, TResponse>` handlers via DI.
- Added `Semaphore` for thread-pool slot management used by the conductor.
- Added `BaseCommandHandler` with async request execution and structured error handling.
- Added `LoggerExtensions` with `LoggerMessage.Define`-based structured logging helpers.
- Extended `StringExtensions`: `UppercaseFirst`, `Truncate`, `GetStableHashCode`, `SanitizeFileName`, `ReplaceChars`, `FindHtmlPart`.
- Added `Contract` class for unconditional argument validation.
- Added localized resource files (`Messages.resx`, `Messages.nl.resx`).
- Added unit tests for `Contract`, `Conductor`, `Semaphore`, `HttpResult`.

### 1.5.2

- Added `net10.0` as a target framework.
- All `Microsoft.Extensions.*` packages upgraded to 10.0.5.
- `System.IdentityModel.Tokens.Jwt` upgraded to 8.17.0.
- `RandomStringGenerator` uses `RandomNumberGenerator.GetInt32` on `net9.0`+ for cryptographically secure output.
- `N2.Core.Abstractions` dependency updated to 1.5.2.

### 1.6.0

- Fixed threading issues in `BaseCommandHandler`, `Conductor`, and `Semaphore`.
- Fixed race condition in `DefaultValueService`.
- Stability improvements in `OAuthCommandHandler` under concurrent load.

### 1.6.1

- **Security:** `Contract.NotNull` is now unconditional — guards no longer stripped in Release builds.
- **Security:** `BackgroundWorker.IsActivated` changed from `static` to instance field — multiple workers no longer share activation state.
- **Security:** JWT validation in `GetPrincipalFromJwt` and `GetPrincipal` now accepts optional `issuer` and `audience` parameters; when supplied, the token is validated against both values.
- **Security:** Basic auth credential parsing in `OAuthCommandHandler` hardened — uses `IndexOf(':')` with explicit guards; passwords containing colons are now handled correctly.
- **Security:** TOTP time window parameter renamed from minutes to seconds across `ValidateTOTP` and `TOTP`; window is now driven by `OAuthConfig.ReplayWindowInSeconds` (default 30 s) rather than a hard-coded value.
- **Security:** `CheckHash` replaced with a constant-time XOR-accumulation comparison; the 500 ms timing floor is now applied once per `ValidateTOTP` call instead of per hash check, eliminating the prior infinite-loop bug on a successful match.
- **Security:** `OAuthConfig.Secret` and `OAuthConfig.Issuer` are now immutable after construction (`private set`).
- Added XML documentation to `Validator`, `OAuthConfig`, and `JwtTools`.
- Added `ReplayWindowInSeconds` property to `OAuthConfig` (configurable via `OAuthConfig:ReplayWindowInSeconds`).
- Added unit tests: `WithValidator`, `WithBackgroundWorker`, `WithAzureFunctionClient`, `UsingWaitForObject`, `WithOAuthCommandHandler`; expanded `JwtToolsTests` and `StringExtensionTests`.
