[![Build Status](https://dev.azure.com/gjkaal/N2%20Vault%20services/_apis/build/status%2FN2.Core%20-%20Publish?branchName=trunk)](https://dev.azure.com/gjkaal/N2%20Vault%20services/_build/latest?definitionId=3&branchName=trunk)

# N2.Core

N2.Core is a multi-target library (`netstandard2.0`, `netstandard2.1`, `net10.0`, `net11.0`) providing reusable infrastructure for ASP.NET Core applications. It covers authentication, command dispatching, HTTP abstractions, geo types, telemetry, and a set of general-purpose extensions -- all designed to be composed without taking on heavy framework dependencies.

This repository depends on a private Azure Artifacts feed. See [docs/private-feed.md](docs/private-feed.md) for setup instructions.

## Functional areas

### Commands (`N2.Core.Commands`)

The command dispatcher (`Conductor`) routes any `IRequest` implementation to its matching `IHandle<TRequest, TResponse>` handler, resolved through the DI container. A thread-pool `Semaphore` limits concurrency so a burst of commands cannot exhaust available threads. Consumer code implements `IHandle`, registers handlers via `services.AddCommandHandlers(...)`, and dispatches through `IConductor` — the coordinator handles the rest. `BaseCommandHandler` provides a base with structured error handling and async execution built in.

### Identity (`N2.Core.Identity`)

`JwtTools` generates and validates JWT tokens and supports TOTP-based one-time passwords, with configurable issuer/audience validation and a constant-time hash comparison to prevent timing attacks. `OAuthCommandHandler` covers the OAuth credential flow, including hardened Basic auth parsing. `ClaimServiceExtensions` provides helpers for reading typed claim values from the current user context.

### Http (`N2.Core.Http`)

`HttpResult` is a uniform response wrapper used across all API endpoints. `HttpContextProxy`, `HttpRequestProxy`, and `HttpResponseProxy` wrap the ASP.NET Core HTTP primitives behind testable interfaces, allowing service and handler code to be tested without a running web host.

### Entity (`N2.Core.Entity`)

`EntityConnectionService` resolves Entity Framework connection strings from the application configuration, centralising connection-string management so individual repositories do not need to read configuration directly.

### Extensions (`N2.Core.Extensions`)

`ServiceCollectionExtensions` provides DI registration helpers — in particular `AddCommandHandlers`, which scans an assembly for `IHandle` implementations and registers them. Beyond DI, this module contains extension methods on `string` (hashing, truncation, file-name sanitisation, HTML extraction), `int`, arrays, `ILogger` (structured logging via `LoggerMessage.Define`), and `HttpClient`.

### Geo (`N2.Core.Geo`)

`GeoCoordinate` and `GeoPosition` are lightweight value types for geographic coordinates. They carry no external dependencies and are designed to be embedded in domain models and DTOs without pulling in a mapping library.

### Telemetry (`N2.Core.Telemetry`)

`ActivityLog` records structured activity entries. `ActivityLoggerFactory` creates loggers that write to the activity log, allowing discrete application events (user actions, background job completions) to be captured separately from diagnostic logging.

### System abstractions (`N2.Core.SystemAbstractions`)

`ITimeSystem` and its default implementation `TimeSystem` abstract `DateTime.UtcNow` so time-dependent logic can be unit-tested without real clock dependencies. `AnonymousUserProfile` is a sentinel value representing an unauthenticated user, used where a non-null user context is always expected.

### Helpers (`N2.Core.Helpers`)

`FileHelpers` provides file-system utilities. `RandomNames` generates human-readable random identifiers useful for test data, temporary resource names, and display purposes.

### Core services

`SettingsService` loads and exposes typed configuration sections from `IConfiguration`. `LocalizedTextService` retrieves localised strings via `ITextService`. `Contract` provides unconditional guard clauses (always active in Release builds). `Validation` exposes composable validation rules. `Semaphore` enforces a concurrency limit on the thread pool. `BackgroundWorker` runs fire-and-forget tasks with controlled activation. `AzureFunctionClient` is an HTTP client wrapper for invoking Azure Functions from within the application.

## Project documentation

| Document | Description |
|----------|-------------|
| [src/DEVELOPER-GUIDE.md](src/DEVELOPER-GUIDE.md) | Developer guide — build commands, CI/CD pipelines, and maintenance notes. |
| [src/WARNINGS.md](src/WARNINGS.md) | Suppressed analyser warnings — lists every `<NoWarn>` entry with severity rating and guidance. |
| [CHANGELOG.md](CHANGELOG.md) | Full version history. |

## Change log

See [CHANGELOG.md](CHANGELOG.md) for the full history. Current version: **2.0.0**.
