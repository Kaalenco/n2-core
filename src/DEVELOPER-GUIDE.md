# N2.Core — Developer Guide

This document explains how to build, test, and maintain the project locally and in CI.

## Solution structure

```
n2-core/
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

## CI/CD

GitHub Actions workflow `.github/workflows/dotnet.yml` builds, tests and publishes to `vault-packages` on push to `trunk`.

## Regular maintenance

### Runtime and SDK targets

The library currently targets `netstandard2.0`, `netstandard2.1`, `net10.0`, and `net11.0`; the unit test project targets `net10.0` and `net11.0`. The GitHub Actions workflow (`.github/workflows/dotnet.yml`) installs the .NET 11.0.x and 10.0.x SDKs. To add or drop a target:

1. Edit `<TargetFrameworks>` in `src/N2.Core/N2.Core.csproj`.
2. Update the SDK installation in `.github/workflows/dotnet.yml` if the new SDK is not already installed.
3. Review `<NoWarn>` entries that exist solely because of older framework limitations (e.g. `IDE0057`, `CA1510`) — they may be removable once older targets are dropped.
4. Update `src/WARNINGS.md` to reflect any suppression changes.

### Package version

The version is set in `src/N2.Core/N2.Core.csproj` under `<Version>`, `<ProductVersion>`, `<PackageVersion>`, and `<AssemblyVersion>` (plus `<FileVersion>`; `<AssemblyVersion>` stays at the major, e.g. 3.0.0.0). Bump these before pushing to `trunk` so the publish workflow produces a correctly versioned package. The `--skip-duplicate` flag in the "Publish to vault-packages" step means pushing an already-published version is a safe no-op, but it will not overwrite the existing package.

Add a matching entry to `CHANGELOG.md` (see the format in that file) in the same commit as the version bump.

### Dependencies

NuGet audit is enabled (`NuGetAudit=True`, `NuGetAuditLevel=Low`), so dependency advisories surface at restore time. For security advisories: bump the affected package, verify the tests pass locally, and note the advisory in the `CHANGELOG.md` entry. For routine updates: bump packages in a dedicated commit separate from feature work so the diff is easy to review.
