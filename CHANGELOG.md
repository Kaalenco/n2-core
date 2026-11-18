# Changelog

## 2.0.0 - 2026-11-18

Major release on N2.Core.Abstractions 2.0.0. Every asynchronous member takes a required
`CancellationToken`, the `Async` suffix is gone from member names, and the target frameworks
move to .NET 10 / .NET 11. All changes below are breaking unless stated otherwise.

### Convention

- Every public member returning `Task` takes a `CancellationToken` as its last parameter. The
  parameter is required: no default value and no overload without it. Tokens are passed through
  to every awaited call that accepts one.
- No member name ends in `Async`.

### Targets and packaging

- Target frameworks are now `netstandard2.0`, `netstandard2.1`, `net10.0` and `net11.0`.
  `net8.0` is dropped; .NET 8 and .NET 9 consumers resolve the `netstandard2.1` asset.
- References `N2.Core.Abstractions` 2.0.0 (was 1.7.1); see its changelog for the renamed and
  re-signed interface members.
- `Microsoft.Extensions.*` packages follow the framework band of the target: 10.0.15 for the
  `netstandard` targets and `net10.0`, 11.0.0 for `net11.0` (was 10.0.7 everywhere).
- `Microsoft.Extensions.Configuration.Abstractions` and `Microsoft.Extensions.Caching.Abstractions`
  are no longer referenced for `net11.0`: they are in-box in `Microsoft.NETCore.App` from
  net11.0 (avoids NU1510 under package pruning).
- Package updates (not breaking): `Microsoft.AspNetCore.Http` 2.3.9 -> 2.3.14,
  `System.IdentityModel.Tokens.Jwt` 8.18.0 -> 8.23.0, `System.IO.Abstractions` 22.1.1 -> 23.0.1.
- **Security:** direct reference to `Microsoft.AspNetCore.WebUtilities` 2.3.14 to lift the
  transitive 2.3.13 from `Microsoft.AspNetCore.Http` (GHSA-6527-9fp8-r4x6, moderate, DoS).
- Unit tests target `net10.0` and `net11.0`; test packages updated (MSTest 4.5.3,
  Microsoft.NET.Test.Sdk 18.12.0, Moq 4.21.0, coverlet.collector 10.1.1).
- Pipelines: .NET SDK 11.0.x replaces 8.0.x (10.0.x kept); coverage runs on `net10.0`.

### Commands (`N2.Core`)

- `BaseCommandHandler<TQ, TA>.HandleRequestAsync(TQ)` (abstract) ->
  `HandleRequest(TQ, CancellationToken)`. Every derived handler must change its override.
- `BaseCommandHandler<TQ, TA>.WaitForAsync(TQ)` -> `WaitFor(TQ, CancellationToken)` (implements
  `ICommandHandler<TQ, TA>.WaitFor`); the token is passed to `HandleRequest`.
- Behaviour: the synchronous path (`ExecuteCommand` / `ICommandHandler.Invoke`) now passes a token
  to `HandleRequest` and cancels it when `TimeoutInMilliSeconds` expires. Before, a timed-out
  handler could not be told to stop. An exception thrown by a cancellation callback is logged
  and the `TimeOut` response is still returned.

### Identity (`N2.Core.Identity`)

- `OAuthCommandHandler.HandleRequestAsync(TokenRequest)` ->
  `HandleRequest(TokenRequest, CancellationToken)`; the token is passed to every
  `IIdentityManager` call (`LogonUser`, `LogonUserWithSecret`, `GetUserSecret`,
  `RefreshUserToken`, `LogoffUser`, `RegisterRefreshToken`, `GetClaims`).

### Core (`N2.Core`)

- `AzureFunctionClient.CallAsync<TQ, TA>(string, TQ, CancellationToken)` ->
  `CallFunction<TQ, TA>(string, TQ, CancellationToken)` (implements `IAzureFunctionClient`); it
  now calls `IHttpClient.Post`.
- `WaitForObject.WaitAsync()` -> `Wait(CancellationToken)`.
- `HttpClientExtensions.ReadJsonDocumentAsync(HttpClient, Uri)` ->
  `ReadJsonDocument(HttpClient, Uri, CancellationToken)`.
- **Fix (not breaking):** `RandomStringGenerator` and `RandomNames` now use
  `RandomNumberGenerator.GetInt32` on all `net*` targets (`#if NETSTANDARD2_1_OR_GREATER || NET`).
  The 1.5.2 entry claimed this for net9.0+, but `NETSTANDARD2_1_OR_GREATER` is not defined for
  `net*` builds, so every `net*` build used the `netstandard2.0` fallback.

### HTTP (`N2.Core.Http`)

- `HttpRequestProxy.ReadAsStringAsync(CancellationToken = default)` ->
  `ReadAsString(CancellationToken)`; the token is now required.
- `HttpResponseProxy.WriteAsync(string, CancellationToken = default)` ->
  `Write(string, CancellationToken)`; the token is now required.
- `HttpResponseProxy.WriteAsync(byte[], CancellationToken = default)` ->
  `Write(byte[], CancellationToken)`; the token is now required.
- `HttpResponseProxy.WriteAsJsonAsync<T>(T, JsonSerializerOptions, CancellationToken = default)` ->
  `WriteAsJson<T>(T, JsonSerializerOptions, CancellationToken)`; the token is now required.
- `HttpResult.WriteJsonResponse` now calls `IHttpResponse.WriteAsJson` (signature unchanged).

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
