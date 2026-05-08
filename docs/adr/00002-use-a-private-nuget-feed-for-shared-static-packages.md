# 00002. Use a private NuGet feed for shared static packages

2026-05-08

## Status

__Accepted__

## Context

Several solutions in this organisation share foundational libraries — things like `N2.Core` and `N2.Core.Abstractions` — that change infrequently and are consumed as versioned dependencies rather than edited in place. Copying source across repositories creates drift; referencing projects directly couples unrelated solutions to each other's build state.

Publishing to nuget.org is unsuitable because these packages are internal, not intended for public consumption, and may contain organisation-specific conventions or configuration.

## Decision

We use a private Azure Artifacts NuGet feed (`vault-packages`) hosted in the `gjkaal` Azure DevOps organisation as the distribution channel for shared, relatively stable packages.

Feed URL:
```
https://pkgs.dev.azure.com/gjkaal/3b3decf8-eba1-45d0-b47a-d9c1ce84f739/_packaging/vault-packages/nuget/v3/index.json
```

Packages published to this feed must:
- be versioned following SemVer — breaking changes increment the major version,
- have a stable, well-defined public API before being published,
- not carry solution-specific business logic.

Developer machines authenticate via the Azure Artifacts Credential Provider (see [docs/private-feed.md](../private-feed.md)). CI pipelines authenticate using a PAT stored as a pipeline secret.

## Consequences

- Shared libraries are consumed as immutable versioned packages; consuming solutions stay decoupled from each other's source trees.
- Each consuming solution must configure the feed in its `nuget.config` or global NuGet sources.
- Developers need the credential provider installed once per machine.
- Package authors own the release process: bumping a version and pushing to the feed is a deliberate act, not a side-effect of a build.
