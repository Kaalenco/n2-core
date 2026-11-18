# Migrating to N2.Core 2.0.0

This guide is for developers upgrading a codebase that consumes `N2.Core` 1.x (last release
1.6.3) to 2.0.0. The full list of changes is in [CHANGELOG.md](CHANGELOG.md); this guide explains
how to apply them.

N2.Core 2.0.0 depends on **N2.Core.Abstractions 2.0.0**, which renames and re-signs the
interfaces N2.Core implements and consumes. The complete Abstractions-level list (Entity, Dms,
Http, Identity, Core interfaces) lives in the Abstractions migration guide:
[N2.Core.Abstractions MIGRATION-2.0.md](https://dev.azure.com/gjkaal/N2%20Vault%20services/_git/N2.Core.Abstractions?path=/MIGRATION-2.0.md&version=GBtrunk).
This guide covers the N2.Core types and repeats only the Abstractions changes that N2.Core
consumers hit most often (section 3.6).

## 1. Summary

**What changed.** The same two conventions as in N2.Core.Abstractions 2.0.0:

- Every public member returning `Task` takes a `CancellationToken` as its **last** parameter. The
  token is **required**: no default value and no overload without it. N2.Core passes the token on
  to every awaited call that accepts one.
- No member name ends in `Async`.

**Who is affected.**

- Everyone with a class deriving from `BaseCommandHandler<TQ, TA>`: the abstract
  `HandleRequestAsync(TQ)` is now `HandleRequest(TQ, CancellationToken)`. This is the change
  most consumers hit.
- Callers of `WaitForAsync`, `AzureFunctionClient.CallAsync`, `WaitForObject.WaitAsync`,
  `HttpClientExtensions.ReadJsonDocumentAsync` and the HTTP proxy methods.
- Implementers of the Abstractions interfaces N2.Core consumes, notably `IIdentityManager` (used
  by `OAuthCommandHandler`) and `IHttpClient` (used by `AzureFunctionClient`).
- Handlers executed through the synchronous command path (`ExecuteCommand` / `Invoke`) get a
  token that is cancelled on timeout (section 4).

**Effort.** Mostly mechanical: the compiler reports every break (`CS0115` for an override that no
longer matches, `CS0534` for the missing abstract member, `CS1061`/`CS7036` for callers). Budget a
few minutes per command handler, plus time to review handlers that run on the synchronous path
for the new cancellation-on-timeout behaviour.

**Target frameworks.**

| 1.6.3 | 2.0.0 |
| --- | --- |
| `netstandard2.0` | `netstandard2.0` (kept) |
| `netstandard2.1` | `netstandard2.1` (kept) |
| `net8.0` | dropped |
| `net10.0` | `net10.0` |
| -- | `net11.0` (new) |

.NET 8 and .NET 9 applications resolve the `netstandard2.1` asset.

**Minimum SDK.** The SDK that matches your own target framework: .NET 10 SDK (10.0.x) for
`net10.0`, .NET 11 SDK (11.0.x) for `net11.0`. Projects on .NET 8/9 consume the `netstandard`
assets and need no new SDK. Building this repository itself needs both the 10.0.x and 11.0.x
SDKs.

## 2. Upgrade checklist

1. **Update both package references together.** N2.Core 2.0.0 requires N2.Core.Abstractions
   2.0.0; mixing majors fails to restore or to compile.

   ```xml
   <PackageReference Include="N2.Core" Version="2.0.0" />
   <PackageReference Include="N2.Core.Abstractions" Version="2.0.0" />
   ```

   With central package management, change the versions in `Directory.Packages.props`. Both
   packages are published to the `vault-packages` Azure Artifacts feed only (section 4).

2. **Retarget if you were on `net8.0` and want a framework-specific asset.** Change
   `<TargetFramework>` to `net10.0` or `net11.0`, or stay on `net8.0`/`net9.0` and use the
   `netstandard2.1` asset. Install the matching SDK and update `global.json` if you pin one.

3. **Align package versions** to avoid NuGet downgrade errors (`NU1605`): raise your own
   references to at least the versions N2.Core now brings (section 4), in particular
   `Microsoft.Extensions.*` to 10.0.15 (on `net10.0` and `netstandard`) or 11.0.0 (on `net11.0`),
   `System.IO.Abstractions` to 23.0.1 and `System.IdentityModel.Tokens.Jwt` to 8.23.0.

4. **Fix the compile errors.**
   - Rename every `HandleRequestAsync(TQ)` override to `HandleRequest(TQ, CancellationToken)`.
   - Rename callers and add a token (section 3).
   - Apply the Abstractions renames in your own implementations (section 3.6 and the
     Abstractions guide).

5. **Pass real tokens.** Inside handlers, pass `cancellationToken` to every awaited call that
   accepts one. Replace placeholder `CancellationToken.None` arguments with a token from the
   calling context (section 5).

6. **Review handlers that run on the synchronous path** (`ExecuteCommand` / `Invoke`): their token
   is now cancelled when `TimeoutInMilliSeconds` expires (default 200 ms). Check
   `TimeoutInMilliSeconds` and what a cancelled handler leaves behind (section 4).

7. **Search for leftovers** with the patterns in section 6, then run your tests.

## 3. Breaking changes

### 3.1 Commands (`N2.Core`)

| 1.x member | 2.0.0 member |
| --- | --- |
| `BaseCommandHandler<TQ, TA>.HandleRequestAsync(TQ)` (abstract) | `HandleRequest(TQ, CancellationToken)` (abstract) |
| `BaseCommandHandler<TQ, TA>.WaitForAsync(TQ)` | `WaitFor(TQ, CancellationToken)` (implements `ICommandHandler<TQ, TA>.WaitFor`) |

`ExecuteCommand(TQ)`, `Invoke()`, `Response(bool)`, `CanHandle(ICommandRequest)` and
`TimeoutInMilliSeconds` are unchanged.

Implementing a handler:

```csharp
// 1.x
public sealed class CreateOrderHandler : BaseCommandHandler<CreateOrderRequest, AcknowledgeResponse>
{
    public CreateOrderHandler(ILogger<CreateOrderHandler> logger, IRuntimeValidator<CreateOrderRequest> validator, IUnitOfWork unitOfWork)
        : base(logger, validator) => this.unitOfWork = unitOfWork;

    public override async Task<AcknowledgeResponse> HandleRequestAsync(CreateOrderRequest request)
    {
        ...
        await unitOfWork.Complete();
        return new AcknowledgeResponse();
    }

    public override bool CanHandle(ICommandRequest command) => command is CreateOrderRequest;
}

// 2.0.0
public sealed class CreateOrderHandler : BaseCommandHandler<CreateOrderRequest, AcknowledgeResponse>
{
    public CreateOrderHandler(ILogger<CreateOrderHandler> logger, IRuntimeValidator<CreateOrderRequest> validator, IUnitOfWork unitOfWork)
        : base(logger, validator) => this.unitOfWork = unitOfWork;

    public override async Task<AcknowledgeResponse> HandleRequest(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        ...
        await unitOfWork.Complete(cancellationToken);
        return new AcknowledgeResponse();
    }

    public override bool CanHandle(ICommandRequest command) => command is CreateOrderRequest;
}
```

Calling a handler asynchronously:

```csharp
// 1.x
AcknowledgeResponse response = await handler.WaitForAsync(request);

// 2.0.0
AcknowledgeResponse response = await handler.WaitFor(request, cancellationToken);
```

`WaitFor` passes the caller's token straight to `HandleRequest`; it applies no timeout of its own.

### 3.2 Identity (`N2.Core.Identity`)

| 1.x member | 2.0.0 member |
| --- | --- |
| `OAuthCommandHandler.HandleRequestAsync(TokenRequest)` | `HandleRequest(TokenRequest, CancellationToken)` |

`OAuthCommandHandler` passes the token to every `IIdentityManager` call (`LogonUser`,
`LogonUserWithSecret`, `GetUserSecret`, `RefreshUserToken`, `LogoffUser`, `RegisterRefreshToken`,
`GetClaims`). Your `IIdentityManager` implementation must take the new token parameter on all of
these (section 3.6) and should pass it on to its database calls.

```csharp
// 1.x
TokenResponse response = await oauthHandler.WaitForAsync(tokenRequest);

// 2.0.0 (minimal API endpoint; the token binds to HttpContext.RequestAborted)
app.MapPost("/token", async (TokenRequest tokenRequest, OAuthCommandHandler oauthHandler, CancellationToken cancellationToken) =>
    await oauthHandler.WaitFor(tokenRequest, cancellationToken));
```

### 3.3 Core (`N2.Core`)

| 1.x member | 2.0.0 member |
| --- | --- |
| `AzureFunctionClient.CallAsync<TQ, TA>(string, TQ, CancellationToken)` | `CallFunction<TQ, TA>(string, TQ, CancellationToken)` |
| `WaitForObject.WaitAsync()` | `Wait(CancellationToken)` |
| `HttpClientExtensions.ReadJsonDocumentAsync(this HttpClient, Uri)` (`N2.Core.Extensions`) | `ReadJsonDocument(this HttpClient, Uri, CancellationToken)` |

`CallFunction` is not simply `Call` because `Call` is a Visual Basic keyword (analyzer CA1716);
the Abstractions guide explains this and the matching `IHttpClient.GetAsync` ->
`GetResponseMessage` rename. `AzureFunctionClient` now calls `IHttpClient.Post`, so your
`IHttpClient` implementation must have the 2.0.0 member names.

```csharp
// 1.x
OrderResult? result = await functionClient.CallAsync<OrderRequest, OrderResult>("api/orders", request, token);
using JsonDocument json = await httpClient.ReadJsonDocumentAsync(uri);
bool ready = await waitForObject.WaitAsync();

// 2.0.0
OrderResult? result = await functionClient.CallFunction<OrderRequest, OrderResult>("api/orders", request, token);
using JsonDocument json = await httpClient.ReadJsonDocument(uri, cancellationToken);
bool ready = await waitForObject.Wait(cancellationToken);
```

### 3.4 HTTP (`N2.Core.Http`)

| 1.x member | 2.0.0 member |
| --- | --- |
| `HttpRequestProxy.ReadAsStringAsync(CancellationToken = default)` | `ReadAsString(CancellationToken)` |
| `HttpResponseProxy.WriteAsync(string, CancellationToken = default)` | `Write(string, CancellationToken)` |
| `HttpResponseProxy.WriteAsync(byte[], CancellationToken = default)` | `Write(byte[], CancellationToken)` |
| `HttpResponseProxy.WriteAsJsonAsync<T>(T, JsonSerializerOptions, CancellationToken = default)` | `WriteAsJson<T>(T, JsonSerializerOptions, CancellationToken)` |

The token is now required. These proxy methods still throw `NotImplementedException`, as in 1.x;
the rename matters mainly for test doubles and wrappers. `HttpResult.WriteJsonResponse(IHttpContext,
JsonSerializerOptions, CancellationToken)` keeps its signature and now calls
`IHttpResponse.WriteAsJson`, so a custom `IHttpResponse` must implement the 2.0.0 name.

### 3.5 Types that did not change

`Conductor`, `Semaphore`, `BackgroundWorker`, `JwtTools`, `SettingsService`, `Contract`, the
`Geo`, `Telemetry` and `SystemAbstractions` types, and the extension methods other than
`ReadJsonDocument` keep their 1.x signatures.

### 3.6 Abstractions changes N2.Core consumers hit most often

The full list is in the Abstractions guide. These are the ones that usually surface together with
an N2.Core upgrade:

| 1.x member | 2.0.0 member |
| --- | --- |
| `ICommandHandler<TQ, TA>.WaitForAsync(TQ)` | `WaitFor(TQ, CancellationToken)` |
| `IUnitOfWork.Complete()` | `Complete(CancellationToken)` |
| `ICoreDataContextFactory<TContext>.CreateAsync()` / `CreateAsync(string)` | `Create(CancellationToken)` / `Create(string, CancellationToken)` |
| `IIdentityManager.LogonUser`, `LogonUserWithSecret`, `GetUserSecret`, `RefreshUserToken`, `LogoffUser`, `RegisterRefreshToken`, `GetClaims` | same names, extra trailing `CancellationToken` |
| `IUserContextFactory.CreateAsync()` | `Create(CancellationToken)` |
| `IHttpClient.PostAsync(Uri, HttpContent, CancellationToken)` | `Post(Uri, HttpContent, CancellationToken)` |
| `IHttpClient.GetAsync(Uri, CancellationToken)` | `GetResponseMessage(Uri, CancellationToken)` |
| `IHttpClient.*Async` (other members) | same name without `Async`, parameters unchanged |
| `IAzureFunctionClient.CallAsync<TQ, TA>` | `CallFunction<TQ, TA>` |
| `IEmailService.SendEmailAsync(...)` / `SendTestEmailAsync()` | `SendEmail(..., CancellationToken)` / `SendTestEmail(CancellationToken)` |

Implementing `IIdentityManager` for `OAuthCommandHandler`:

```csharp
// 1.x
public async Task<Guid> RefreshUserToken(Guid refresh)
{
    RefreshTokenRecord? record = await db.RefreshTokens.FindAsync(refresh);
    ...
}

// 2.0.0
public async Task<Guid> RefreshUserToken(Guid refresh, CancellationToken cancellationToken)
{
    RefreshTokenRecord? record = await db.RefreshTokens.FindAsync([refresh], cancellationToken);
    ...
}
```

## 4. Behaviour, package and dependency changes

### 4.1 Synchronous command path cancels the handler on timeout

`ExecuteCommand(TQ)` and `ICommandHandler.Invoke(ICommandRequest)` run `HandleRequest` and wait at
most `TimeoutInMilliSeconds` (default `DefaultTimeoutInMilliseconds` = 200 ms; 0 or less means
`LongTimeout`, 10 minutes).

- **1.x:** on timeout the handler kept running on the thread pool with no way to stop it; the
  caller got a `TimeOut` response while side effects (database writes, HTTP calls) continued.
- **2.0.0:** `HandleRequest` receives a token from an internal `CancellationTokenSource`, which is
  cancelled when the timeout expires. The caller still gets the `TimeOut` response. A handler that
  observes the token stops (usually with an `OperationCanceledException` from an awaited call); a
  handler that ignores it keeps running as before.
- If a callback registered on that token throws, the exception is logged and the `TimeOut`
  response is still returned.
- The token is not linked to any caller token: the synchronous path has none.

What to check:

- **The 200 ms default is short.** A handler that used to "time out" but still finish its work in
  the background now gets cancelled midway once it passes the token on. Set
  `TimeoutInMilliSeconds` to a realistic value for each handler used on this path, or call
  `WaitFor` with a caller token instead.
- **Make cancellation safe.** Keep related writes in one unit of work and commit them with a
  single `IUnitOfWork.Complete(cancellationToken)` at the end, so a handler cancelled before that
  point leaves nothing applied. If a step must complete regardless (for example an audit record), pass
  `CancellationToken.None` to that one call deliberately and comment why.

`WaitFor` is unchanged in behaviour apart from passing the caller's token on.

### 4.2 Other behaviour changes

- `WaitForObject.Wait(CancellationToken)`: a cancelled token ends the wait with an
  `OperationCanceledException` (`TaskCanceledException`) instead of looping until the condition or
  the timeout is reached.
- `HttpClientExtensions.ReadJsonDocument`: the request and the JSON parse are cancellable. On the
  `netstandard` targets reading the response stream itself is not (no token overload there).
- `OAuthCommandHandler`: when the token is cancelled during the 200 ms wait for the refresh-token
  registration, the access token is returned without a refresh token, the same as when the
  registration is slow.
- **RNG on net10/net11:** `RandomStringGenerator.Generate` and `RandomNames.CreateRandomName` now
  use `RandomNumberGenerator.GetInt32` on all `net*` targets. In 1.x every `net*` build used the
  `netstandard2.0` fallback (a cryptographic RNG with a modulo bias), because the conditional only
  matched `netstandard2.1`. The output format is unchanged; values are now uniformly distributed.
  `RandomStringGenerator` is still not meant for security purposes.

### 4.3 Packages and dependencies

| Package | 1.6.3 | 2.0.0 |
| --- | --- | --- |
| `N2.Core.Abstractions` | 1.7.1 | 2.0.0 |
| `Microsoft.Extensions.*` (Configuration, Binder, Json, UserSecrets, EnvironmentVariables, DependencyInjection, Logging, Logging.Console, Caching.Memory) | 10.0.7 | 10.0.15 for `netstandard` and `net10.0`; 11.0.0 for `net11.0` |
| `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Caching.Abstractions` | 10.0.7 | 10.0.15 for `netstandard` and `net10.0`; not referenced on `net11.0` (in-box) |
| `Microsoft.AspNetCore.Http` | 2.3.9 | 2.3.14 |
| `Microsoft.AspNetCore.WebUtilities` | transitive | 2.3.14, direct (security pin) |
| `System.IdentityModel.Tokens.Jwt` | 8.18.0 | 8.23.0 |
| `System.IO.Abstractions` | 22.1.1 | 23.0.1 |

- **Framework bands.** `Microsoft.Extensions.*` follow the band of the target framework. A
  `net10.0` application stays on 10.0.x and is not lifted to 11.0.0 assemblies that conflict with
  the ASP.NET Core 10 shared framework; a `net11.0` application gets 11.0.0.
- **In-box on net11.** `Microsoft.Extensions.Configuration.Abstractions` and
  `Microsoft.Extensions.Caching.Abstractions` are part of `Microsoft.NETCore.App` from net11.0, so
  N2.Core does not reference them there (a direct reference is pruned and raises `NU1510`). You
  can drop your own explicit references to them on `net11.0` as well.
- **Security pin.** `Microsoft.AspNetCore.Http` 2.3.14 still brings
  `Microsoft.AspNetCore.WebUtilities` 2.3.13 transitively, which is affected by
  GHSA-6527-9fp8-r4x6 (moderate, denial of service). N2.Core references WebUtilities 2.3.14
  directly to lift it. If your project pins WebUtilities itself, use 2.3.14 or later.
- **Publishing.** 2.0.0 is published to the `vault-packages` Azure Artifacts feed only; a
  nuget.org release is deferred. Make sure the consuming solution's `nuget.config` has the
  `vault-packages` source (see [docs/private-feed.md](docs/private-feed.md)).

## 5. Where to get a CancellationToken

Take the token from the outermost caller and pass it down through every layer. Give your own
async methods a `CancellationToken cancellationToken` parameter (last, no default) rather than
creating tokens in the middle of a call chain.

- **ASP.NET Core minimal API or controller:** add a `CancellationToken` parameter to the endpoint
  or action; it binds to `HttpContext.RequestAborted`. In middleware and filters use
  `HttpContext.RequestAborted` directly.
- **Blazor component:** components have no built-in token. Own a `CancellationTokenSource`,
  pass its `Token`, and cancel and dispose it in `Dispose`/`DisposeAsync`:

  ```razor
  @implements IDisposable

  @code {
      private readonly CancellationTokenSource disposalSource = new();

      private async Task Save()
          => response = await handler.WaitFor(request, disposalSource.Token);

      public void Dispose()
      {
          disposalSource.Cancel();
          disposalSource.Dispose();
      }
  }
  ```

- **Background service:** the `stoppingToken` of `BackgroundService.ExecuteAsync`, or the token
  passed to `IHostedService.StartAsync`/`StopAsync`.
- **Azure Functions (isolated worker):** a `CancellationToken` function parameter or
  `FunctionContext.CancellationToken`.
- **Command handlers:** the `cancellationToken` parameter of `HandleRequest`.
- **Tests:** the framework's token, so a hanging test is cancelled on timeout or abort --
  MSTest `TestContext.CancellationToken`, NUnit `TestContext.CurrentContext.CancellationToken`,
  xUnit v3 `TestContext.Current.CancellationToken`. With Moq, match the new parameter with
  `It.IsAny<CancellationToken>()`.

  ```csharp
  [TestMethod]
  public async Task CreatesOrder()
  {
      unitOfWork.Setup(u => u.Complete(It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResponseStatus.Success, (string?)null));

      AcknowledgeResponse response = await handler.WaitFor(request, TestContext.CancellationToken);
      ...
  }
  ```

**Avoid `CancellationToken.None`** except at a true top-level entry point that has no token to
offer (a `Main` without a host, a one-off tool), or for a single call that must complete even when
the caller cancels, with a comment explaining why. Elsewhere it compiles but silently opts the
call out of cancellation: work for a request the client has abandoned keeps running.

## 6. Finding affected call sites

The compiler finds every break; these searches help to scope the work and to catch placeholder
tokens afterwards. Patterns are regular expressions (for example
`git grep -nE '<pattern>' -- '*.cs' '*.razor'`). BCL and EF Core methods keep their `Async`
suffix, so expect hits that need no change.

| Pattern | Finds |
| --- | --- |
| `HandleRequestAsync` | `BaseCommandHandler` overrides and direct calls |
| `WaitForAsync\(` | `WaitFor` callers (`BaseCommandHandler`, `ICommandHandler<TQ, TA>`) |
| `: BaseCommandHandler<` | handlers to review for the timeout cancellation (section 4.1) |
| `TimeoutInMilliSeconds` | handlers that tune the synchronous-path timeout |
| `\.(ExecuteCommand\|Invoke)\(` | synchronous command path callers |
| `\bCallAsync\b` | `AzureFunctionClient` / `IAzureFunctionClient.CallAsync` -> `CallFunction` |
| `ReadJsonDocumentAsync\(` | `HttpClientExtensions` and `IHttpClient` callers |
| `\.WaitAsync\(\)` | `WaitForObject.WaitAsync` (also matches BCL `Task.WaitAsync`; check the receiver) |
| `(ReadAsStringAsync\|WriteAsJsonAsync\|WriteAsync)\(` | HTTP proxy / `IHttpRequest` / `IHttpResponse` calls |
| `\.Complete\(\)` | `IUnitOfWork.Complete()` without a token |
| `Async\(` / `Async<` | any other former N2 `...Async` member (see the Abstractions guide) |
| `CancellationToken\s+\w+\s*=\s*default` | implementations that still declare an optional token |
| `CancellationToken\.None\|default\(CancellationToken\)` | placeholder tokens to replace (section 5) |
| `<TargetFrameworks?>.*net8\.0` | projects still targeting `net8.0` |
