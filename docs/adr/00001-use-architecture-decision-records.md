# 00001. Use Architecture Decision Records

2026-05-08

## Status

__Accepted__

## Context

Significant design decisions are made throughout the lifetime of a project — during initial setup, during feature work, and when resolving technical debt. Without a record, the reasoning behind these decisions is lost. New contributors cannot tell what was considered and rejected, and revisiting a decision means reconstructing context from scratch.

## Decision

We will document architecturally significant decisions as Architecture Decision Records (ADRs) stored in `docs/adr/`. A decision is architecturally significant when it affects the structure, non-functional characteristics, dependencies, interfaces, or construction techniques of the system.

Each ADR is a short Markdown file that captures:
- the context and forces at play at the time of the decision,
- the decision itself,
- and its consequences (positive and negative).

ADRs are immutable once accepted. Superseded decisions are marked as such and linked to the ADR that replaces them. The table of contents in `docs/adr-toc.md` is kept up to date.

Tooling: [adr-cli](https://github.com/gjkaal/adr-cli) — configured via `adr.config.json` in the repository root.

## Consequences

- Decisions and their rationale are traceable over time.
- New contributors can read the ADR log to understand why the codebase looks the way it does.
- Revisiting a decision requires writing a new ADR, which encourages deliberate rather than accidental change.
- There is a small overhead per significant decision; trivial choices do not warrant an ADR.
