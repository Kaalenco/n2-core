# Test coverage notes

This document explains why certain source files in N2.Core do not have corresponding unit tests in this project.

## Script false positive

### `Validation.cs`
The file is named `Validation.cs` but the class inside is `Validator`. Tests exist in `WithValidator.cs` — 19 test methods covering all public methods of `Validator`. The coverage script cannot detect this because the file name and class name differ.

---

## Constants, enums, and plain data types

No logic to test. The only meaningful assertion would be that a constant equals its own hardcoded value, which adds noise without catching real bugs. A value change is immediately visible in the code diff.

| File | What it contains |
|---|---|
| `DefaultValues.cs` | 3 named constants: `DefaultGuid`, `AnonymousUserId`, `UserInteractionTimeout` |
| `DefaultValueService.cs` | Single read-only `JsonSerializerOptions` property |
| `Http/FailureReason.cs` | 4-value enum |
| `Http/SuccessReason.cs` | Enum |
| `WorkerStatus.cs` | 12-property DTO, init-only, no logic |

---

## Null-object pattern

These classes deliberately return null, default, or false for every operation. Their contract is "I exist but do nothing", which is enforced by the interface rather than logic in the class. Any regression surfaces in consuming code, not here.

| File | Interface implemented |
|---|---|
| `NullConfiguration.cs` | `IConfiguration` |
| `NullLoggingService.cs` | Logging service |
| `SystemAbstractions/AnonymousUserProfile.cs` | `IUserProfile` — all role checks return `false`, all collections empty |
| `SystemAbstractions/TimeSystem.cs` | `ITimeSystem` — single-expression properties wrapping `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTime.Today` |

---

## Test infrastructure

**`FakeTextService.cs`** is used as a test double inside this test suite. Testing test helpers would create a circular dependency and adds no value.

---

## Classes requiring ASP.NET Core HttpContext infrastructure

These classes wrap or extend `HttpContext`, `HttpRequest`, or `HttpResponse`. Testing them requires constructing a `DefaultHttpContext` and wiring up request/response streams — setup that is longer than the code under test. The forwarding logic they contain is better validated at the integration level where a real HTTP pipeline runs.

- `Http/HttpContextProxy.cs`
- `Http/HttpRequestProxy.cs`
- `Http/HttpResponseProxy.cs`
- `Http/HttpContextExtensions.cs`
- `Http/HttpContextAccessorExtensions.cs`
- `Http/HttpRequestExtensions.cs`
- `Http/HttpContextExceptions.cs` — two static exception-factory properties; trivial but only meaningful in the HTTP context above
- `Identity/TokenCommandValidator.cs` — reads claims from `HttpContext`

---

## Simple factory with no additional logic

**`Telemetry/ActivityLoggerFactory.cs`** creates an `Activity` and wraps it in an `ActivityLog`. The `ActivityLog` itself is tested in `UsingActivityLog.cs`. The factory adds no logic; the only test would assert "returns non-null", which is already implied by the constructor tests.

---

## Thin wrappers with untestable or framework-owned behaviour

**`StdOutConsole.cs`** delegates to `Console.Out`. Asserting its output requires redirecting the standard output stream, which tests `Console`, not this class.

**`Extensions/LoggerExtensions.cs`** contains 9 extension methods that each forward to a `LoggerMessage.Define`-generated delegate. Each method body is a single expression calling a private static delegate. Testing them means verifying that `ILogger` received the right call — which tests the Microsoft logging infrastructure rather than any logic in this class.

---

## Integration-level concern

**`Extensions/HttpClientExtensions.cs`** (`ReadJsonDocumentAsync`) deserialises an `HttpResponseMessage` body. The interesting failure modes — network errors, malformed JSON, unexpected content types — are better covered by integration or contract tests against a real or faked HTTP endpoint.

---

## Borderline: has logic, deferred

**`Identity/OAuthConfig.cs`** reads 4 configuration keys and falls back to named defaults (`DefaultTokenTimeOut = 20`, `DefaultReplayWindow = 30`) when parsing fails. The constructor has real conditional logic: a null config throws; a missing or unparseable timeout falls back to the constant; a parseable value is used as-is.

Tests should be added if the JWT/OAuth flow becomes a coverage priority. Suggested cases:
- Null `IConfiguration` throws `ArgumentNullException`
- Missing timeout key uses `DefaultTokenTimeOut`
- Parseable timeout key uses the configured value
- Missing replay window key uses `DefaultReplayWindow`
