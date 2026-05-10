# N2.Core — Developer Guide

This document explains how to build, test, and maintain the project locally and in CI.

## Solution structure

```
n2-core/
├── pipelines/
│   ├── n2-core-ci-pr.yml       # PR validation — build and test only
│   └── n2-core-ci-publish.yml  # Trunk pipeline — build, test, pack, publish, and code analysis
├── src/
│   ├── N2.Core/                # Library source
│   ├── N2.Core.UnitTests/      # MSTest unit tests
│   ├── nuget.config            # Points to the private Azure Artifacts feed
│   ├── DEVELOPER-GUIDE.md      # This file
│   └── WARNINGS.md             # Suppressed analyser warnings
├── docs/
│   ├── adr/                    # Architecture Decision Records
│   └── private-feed.md         # One-time feed authentication setup
├── CHANGELOG.md
└── README.md
```

## Building

```bash
# Restore dependencies (requires private feed authentication — see docs/private-feed.md)
dotnet restore src

# Debug build
dotnet build src --no-restore

# Release build
dotnet build src --no-restore --configuration Release
```

The release build sets `GeneratePackageOnBuild=True` (see `N2.Core.csproj`), so it produces a `.nupkg` in the output directory. Pass `/p:GeneratePackageOnBuild=false` to suppress this during local iteration.

## Running tests

### Unit tests

```bash
dotnet test src --no-build --verbosity normal
```

### Single test class

```bash
dotnet test src --no-build --filter "FullyQualifiedName~WithSomeClass"
```

### Single test method

```bash
dotnet test src --no-build --filter "FullyQualifiedName~WithSomeClass.MethodName"
```

Test results are written to the `TestResults/` directory. The `ENV__Secret__Value` environment variable must be set for OAuth-related tests; locally this is done via `dotnet user-secrets` or an `.env` file.

## CI/CD pipeline

There are two Azure DevOps pipeline definitions in `pipelines/`.

### PR pipeline (`n2-core-ci-pr.yml`)

Triggers on every pull request to any branch when files under `src/N2.*/**` change. Documentation-only commits do not trigger a run.

| # | Stage | Description |
|---|-------|-------------|
| 1 | **Build and test** | Restore, build (Release, `GeneratePackageOnBuild=false`), run unit tests, publish code coverage. Retains the run for 3 days. |

### Publish pipeline (`n2-core-ci-publish.yml`)

Triggers on push to `trunk` when files under `src/N2.*/**` change. PR trigger is disabled so PR runs use the PR pipeline exclusively.

| # | Stage | Runs on | Description |
|---|-------|---------|-------------|
| 1 | **Build and test** | Every push to `trunk` | Restore, build, run unit tests, pack, publish the `packages` artifact. Retains the run for 10 days. |
| 2 | **Code analysis** | Every push to `trunk` (parallel with Publish) | Runs tests with coverage via `net8.0`, generates an HTML coverage report with ReportGenerator, generates a cyclomatic complexity report with Lizard (`-C 10`), and writes a pipeline summary. |
| 3 | **Publish** | After Build and test succeeds, `trunk` only | Downloads the `packages` artifact and pushes `.nupkg` files to the `vault-packages` Azure Artifacts feed with `--skip-duplicate`. Retains the run for 365 days. |

### Pipeline variables

| Variable | Value | Notes |
|----------|-------|-------|
| `buildConfiguration` | `Release` | Defined in the YAML. |
| `ENV__Secret__Value` | `NotSoSecret` | Set in the YAML `env:` block for the test step. In production, override via **Pipelines → Edit → Variables**. |
| `SYSTEM_ACCESSTOKEN` | *(pipeline token)* | Injected automatically by Azure DevOps; used by the retention lease script. |

The `vault-packages` feed URL is hard-coded in the Publish stage. Changing the feed requires updating the `FEED_URL` in `n2-core-ci-publish.yml` and granting the build service **Contributor** access on the new feed (see the comment at the top of the publish pipeline file).

## Regular maintenance

### Runtime and SDK targets

The library currently targets `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net10.0`. To add or drop a target:

1. Edit `<TargetFrameworks>` in `src/N2.Core/N2.Core.csproj`.
2. Add a matching `UseDotNet@2` step in both pipeline YAML files if the new SDK is not already installed.
3. Review `<NoWarn>` entries that exist solely because of older framework limitations (e.g. `IDE0057`, `CA1510`) — they may be removable once older targets are dropped.
4. Update `src/WARNINGS.md` to reflect any suppression changes.

### Package version

The version is set in `src/N2.Core/N2.Core.csproj` under `<Version>`, `<ProductVersion>`, and `<PackageVersion>` (all three must match). Bump these before pushing to `trunk` so the publish pipeline produces a correctly versioned package. The `--skip-duplicate` flag in the Publish stage means pushing an already-published version is a safe no-op, but it will not overwrite the existing package.

Add a matching entry to `CHANGELOG.md` (see the format in that file) in the same commit as the version bump.

### Dependencies

NuGet audit is enabled (`NuGetAudit=True`, `NuGetAuditLevel=Low`), so dependency advisories surface at restore time. For security advisories: bump the affected package, verify the tests pass locally, and note the advisory in the `CHANGELOG.md` entry. For routine updates: bump packages in a dedicated commit separate from feature work so the diff is easy to review.
