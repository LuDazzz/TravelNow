# AGENTS.md Clean Architecture and Harness Design

**Status:** Approved design
**Date:** 2026-08-06
**Scope:** Repository-wide operating instructions for coding agents working on TravelNow

## Context

TravelNow is a .NET 10 backend split into five projects:

- `TravelNow.Domain`
- `TravelNow.Application`
- `TravelNow.Infrastructure`
- `TravelNow.Shared`
- `TravelNow` (ASP.NET Core API host)

The repository has the shape of a layered architecture, but its current project and type dependencies do not yet satisfy strict Clean Architecture boundaries. It also has no test projects, repository-wide formatting configuration, architecture tests, or agent operating instructions.

The requested deliverable is one root `AGENTS.md` that acts as an enforceable operating contract for future coding agents. It must define the target architecture, strict remediation policy, task lifecycle, validation evidence, and completion criteria needed for a repeatable harness flow.

## Goals

1. Make ownership and permitted dependencies explicit for every project.
2. Prevent new framework or persistence concerns from leaking into inner layers.
3. Require code that touches an existing violation to remediate the related violation.
4. Give agents a deterministic discover, design, test, implement, verify, review, and report loop.
5. Scale testing and validation to risk without allowing unverified completion claims.
6. Protect secrets, user changes, public contracts, and production data.
7. Record current repository debt so existing code is not mistaken for an approved pattern.

## Non-Goals

- This task does not refactor the existing solution into the target architecture.
- This task does not add test projects, analyzers, packages, CI workflows, or formatting files.
- This task does not repair current nullable warnings or package vulnerabilities.
- This task does not change application behavior, API contracts, database schema, or migrations.
- The root instruction file will not contain a complete onboarding guide or duplicate the README.

## Decision Summary

- Use one authoritative `AGENTS.md` at the repository root.
- Write agent instructions in English for consistent interpretation across coding harnesses.
- Enforce strict Clean Architecture for all new code and all directly touched legacy code.
- Keep project-specific rules in the root file while the repository remains small.
- Split into nested instruction files only when a project gains a genuinely distinct workflow or the root rules become difficult to navigate. Nested files must narrow or extend the root contract, never silently contradict it.

## Repository Baseline

The design is grounded in the repository state observed on 2026-08-06:

- SDK available locally: .NET SDK `10.0.300`.
- `dotnet build TravelNow.slnx --nologo` succeeds after package restore.
- The successful build emits 18 warnings.
- The API dependency graph reports `NU1903` for a known high-severity vulnerability in transitive package `Microsoft.OpenApi` 2.0.0.
- Domain entities emit multiple `CS8618` nullable initialization warnings.
- No test project is present in the solution.
- No root `.editorconfig`, `global.json`, `Directory.Build.props`, or `Directory.Packages.props` is present.
- The working tree was clean before design work began.

These facts are a baseline, not approval to reproduce the current patterns.

## Target Dependency Model

An arrow means "may depend on."

| Project | May depend on | Must not depend on |
| --- | --- | --- |
| `TravelNow.Shared` | BCL and dependency-free technical primitives | Any TravelNow project or business concept |
| `TravelNow.Domain` | BCL; exceptionally a dependency-free Shared primitive | EF Core, ASP.NET Core, Identity, Npgsql, serializers, Infrastructure, API |
| `TravelNow.Application` | Domain and dependency-free Shared primitives | EF Core implementation types, ASP.NET Core, HttpContext, Npgsql, Infrastructure, API |
| `TravelNow.Infrastructure` | Application, Domain, Shared | API presentation types or controller concerns |
| `TravelNow` | Application and Infrastructure; Shared only for host concerns | Business rules or direct persistence behavior in controllers/middleware |

The API project is the composition root. Infrastructure points inward by implementing Application-owned ports. Runtime control flow may move outward through interfaces, but source-code dependencies must continue pointing inward.

## Layer Ownership

### Domain

Domain owns entities, value objects, invariants, domain services, domain events, domain exceptions, and business enums or constants. It must remain persistence- and transport-agnostic. It must not contain EF configuration, persistence attributes, HTTP concepts, logging, serializers, or external-provider SDKs.

### Application

Application owns use cases, commands, queries, input/output models, validation, authorization decisions that depend on business context, and ports required by use cases. Features should be organized as vertical slices inside the Application boundary. Ports must describe application or domain needs and must not expose `DbContext`, `DbSet`, `IQueryable`, provider-specific expressions, HTTP state, or infrastructure models.

Transaction boundaries belong to the use case. A transaction abstraction may expose an operation such as `SaveChangesAsync(CancellationToken)`, but it must not expose the concrete persistence engine.

### Infrastructure

Infrastructure owns EF Core, PostgreSQL, ASP.NET Identity persistence, repository and port implementations, migrations, caches, files, email, queues, and third-party clients. EF mappings belong in `IEntityTypeConfiguration<T>` classes. Infrastructure may translate provider failures into stable Application/Domain failures but must not leak provider exceptions across its boundary.

Audit population and soft-delete implementation remain centralized infrastructure concerns and require integration coverage.

### API Host

The API host owns controllers, middleware, authentication wiring, HTTP authorization policies, request/response mapping, status codes, serialization, OpenAPI, and dependency composition. Controllers must remain thin. They validate transport shape, invoke one use case, and map the result. They must not query a `DbContext`, implement business decisions, or return domain entities directly.

### Shared

Shared contains only broadly reusable, dependency-free technical primitives or helpers. It must not contain business vocabulary, feature behavior, ports, persistence concerns, or code placed there merely because its owner is unclear. A helper that needs a Domain constant is not dependency-free and must be moved, inverted, or redesigned.

## Strict Legacy Remediation Policy

Existing code is not precedent when it conflicts with this design.

- New code must comply fully with the target boundaries.
- A task that directly edits a violating file or depends on a violating contract must remove the related violation as part of the same change.
- Related remediation includes tests and consumers required to keep behavior stable.
- Agents must not add warning suppressions, null-forgiving operators, compatibility wrappers, or copied anti-patterns to avoid remediation.
- Unrelated repository-wide refactoring is still out of scope. If correct remediation would materially expand the requested task, the agent must explain the dependency and obtain direction before proceeding.
- A task must not claim the full repository is clean while baseline debt remains. It must distinguish task-local compliance from repository-wide compliance.

Known violations to call out in `AGENTS.md` include:

- Domain package references to EF Core, Identity, Npgsql, and Newtonsoft.Json.
- Persistence annotations on Domain entities.
- Application package references to persistence/provider libraries.
- `IUnitOfWork` exposing `DbContext`.
- Repository contracts exposing query-provider concerns.
- `TravelNow.Shared` referencing `TravelNow.Domain` for date-format constants.
- The API response envelope living in Application despite representing a presentation contract.
- Missing automated tests and architecture enforcement.
- Existing nullable warnings and the reported package vulnerability.

## Harness Flow

Each non-trivial task follows the stages below. Agents may compress the narration for small tasks, but they may not omit the reasoning or evidence.

### 1. Discover

Entry: a concrete task has been received.

Actions:

- Read the root instructions and any narrower instructions in scope.
- Inspect `git status` before modifying files.
- Locate the relevant entry point, call path, project references, tests, configurations, and recent nearby patterns.
- Read the implementation before proposing changes.
- Identify user-owned uncommitted changes and preserve them.

Exit: the agent can name the owning layer, affected behavior, affected projects, and relevant validation commands.

### 2. Frame

Actions:

- State acceptance criteria and observable behavior.
- State non-goals when scope could expand.
- Classify risk: domain logic, public API, authorization, database, migration, concurrency, external I/O, or configuration.
- Identify architecture debt directly on the change path.
- Create a short plan for multi-step work.

Stop and ask for direction if an unresolved choice could break a public contract, weaken security, destroy or rewrite data, or cause a materially different product behavior.

Exit: scope and completion evidence are explicit.

### 3. Establish Baseline

Actions:

- Run the narrowest relevant existing tests and build before editing when feasible.
- Record pre-existing failures and warnings rather than attributing them to the task later.
- Restore packages only when needed.
- If an environmental restriction blocks verification, retry through the approved mechanism or report the exact limitation.

Exit: baseline health is known.

### 4. Design the Change

Actions:

- Place behavior in the layer that owns the decision.
- Define data and control flow across ports.
- Decide validation, failure semantics, transaction boundary, authorization, cancellation, and observability.
- Prefer the smallest design that satisfies the acceptance criteria without speculative abstraction.

Exit: dependencies obey the target direction and exceptional paths have defined behavior.

### 5. Red

For a behavior change or bug fix, add a test that fails for the expected reason before implementation. A bug fix requires a regression test that reproduces the defect. Documentation-only, formatting-only, and metadata-only changes are exempt from test-first work but still require suitable validation.

Exit: the new test demonstrates the missing or incorrect behavior, not an unrelated setup failure.

### 6. Green

Implement the smallest coherent change that makes the focused test pass. Keep changes within the identified ownership boundaries. Propagate `CancellationToken` through request and I/O paths. Do not weaken assertions or bypass production behavior solely for tests.

Exit: focused behavior passes and the implementation respects dependency rules.

### 7. Refactor

Remove duplication, unclear naming, dead code, and boundary leaks introduced or exposed by the task. Keep behavior covered and green. Refactoring outside the direct change path requires separate justification.

Exit: the changed code is understandable and no longer carries related architecture violations.

### 8. Focused Verification

Run formatting, build, unit tests, and relevant integration or contract tests for affected projects. Inspect generated migrations and model snapshots when the model changes.

Exit: all task-local gates pass without new warnings.

### 9. Full Verification

Run repository-wide gates appropriate to the change. Review dependency direction, package/security output, public API compatibility, database impact, and the complete diff. Do not hide baseline failures; report them distinctly.

Exit: the task has not introduced a regression outside the focused scope.

### 10. Handoff

Report:

- What behavior changed.
- Which important files or boundaries changed.
- Exact validation commands run and their results.
- Migrations, contract changes, configuration, or operational steps.
- Remaining baseline debt or residual risk.
- Any command that could not be run and why.

Completion claims must be supported by fresh command output from the current state.

## C# and .NET Rules

- Keep nullable reference types enabled and model nullability truthfully.
- Do not use `!`, broad suppression, or arbitrary defaults to silence warnings.
- Suffix asynchronous methods with `Async`.
- Do not use `.Result`, `.Wait()`, blocking locks around asynchronous I/O, or `async void` outside event handlers.
- Accept and propagate `CancellationToken` for request-scoped and I/O work.
- Use UTC and `DateTimeOffset` for persisted instants. Inject a clock when time controls behavior.
- Use file-scoped namespaces and keep one primary public type per file.
- Prefer immutable request/result models where practical.
- Name types and members by business intent, not technical mechanism alone.
- Comments explain decisions, invariants, or non-obvious constraints; they do not narrate the code.
- Avoid speculative base classes, generic abstractions, and new packages.
- Validate options/configuration at startup when invalid configuration would make the service unsafe or unusable.

## Persistence and Migration Rules

- Configure schema in Infrastructure, not with persistence attributes in Domain.
- Specify requiredness, maximum lengths, numeric precision, indexes, uniqueness, relationships, and delete behavior explicitly.
- Use projection and `AsNoTracking` for read-only queries.
- Paginate collection endpoints and reject unbounded reads.
- Avoid N+1 queries, accidental client evaluation, and indiscriminate `Include` graphs.
- Keep transactions as short as possible and centered on a use case.
- Do not use EF InMemory to prove PostgreSQL relational behavior.
- Generate migrations through EF tooling; do not hand-edit the snapshot.
- Review generated operations for data loss, locks, table rewrites, defaults, backfills, and reversibility.
- Do not apply a migration to a shared or production database without explicit authorization.
- Never silently enable automatic production migration at application startup.

## API and Contract Rules

- Use dedicated transport request/response models and prevent over-posting.
- Validate syntax at the boundary and enforce business invariants in Domain/Application.
- Keep success and error shapes stable unless the task explicitly versions or changes the contract.
- Map known failures to intentional 4xx responses.
- Return a generic 500 response for unexpected failures and keep details in structured server logs.
- Do not return exception messages, stack traces, SQL, internal paths, or provider details to clients.
- Treat a change to route, status, field name/type/nullability, pagination, error code, or authorization as a contract change requiring coverage and compatibility analysis.

## Security and Observability Rules

- Authentication establishes identity; authorization must separately verify permission and resource ownership.
- Enforce authorization server-side at the appropriate API/Application boundary.
- Validate identifiers and ownership to prevent insecure direct object references.
- Do not log or commit passwords, tokens, connection strings, secrets, or sensitive personal data.
- Store secrets in environment variables, user secrets, or an approved secret store.
- Use structured logging with stable event context; do not build log messages by concatenating untrusted data.
- Domain remains logging-independent.
- Preserve exception context in logs while sanitizing client responses.
- Consider retry, timeout, and idempotency explicitly for external I/O; do not retry non-transient failures blindly.

## Testing Strategy

| Scope | Purpose | Dependencies |
| --- | --- | --- |
| Domain unit tests | Invariants, value objects, domain services and events | No database, network, ASP.NET host, or mocks of domain objects |
| Application unit tests | Use-case orchestration, validation, authorization and failure paths | Fakes/mocks only at owned ports |
| Infrastructure integration tests | EF mappings, queries, constraints, transactions, audit and soft delete | Real PostgreSQL-compatible environment |
| API integration/contract tests | Routing, binding, authentication, authorization, status and response contracts | In-process host plus controlled infrastructure |
| Architecture tests | Project/type dependency rules | Inspect compiled assemblies or project references |

Recommended test project layout as coverage is introduced:

- `tests/TravelNow.Domain.UnitTests`
- `tests/TravelNow.Application.UnitTests`
- `tests/TravelNow.Infrastructure.IntegrationTests`
- `tests/TravelNow.Api.IntegrationTests`
- `tests/TravelNow.ArchitectureTests`

Tests must be deterministic, isolated, parallel-safe where enabled, and independent of execution order. Control time, current user, identifiers, and external providers through explicit boundaries. Integration fixtures must own their setup and cleanup. Avoid shared mutable seed data.

## Quality Gates

The root instructions will define the standard command sequence:

```powershell
dotnet restore TravelNow.slnx
dotnet format TravelNow.slnx --verify-no-changes --no-restore
dotnet build TravelNow.slnx --no-restore
dotnet test TravelNow.slnx --no-build
```

Agents should first run narrower project/test commands for fast feedback, then the full sequence. For relevant tasks, add architecture tests, migration inspection, and a transitive package vulnerability audit.

Warning policy during migration to a clean baseline:

- No new warning is allowed.
- A directly modified project must be warning-free, including warnings caused by its changed contracts.
- A touched violation or warning must be fixed rather than suppressed.
- Unrelated baseline warnings must be listed explicitly until a dedicated remediation task removes them.
- Once the repository reaches zero warnings, all builds use warnings-as-errors and the baseline may not regress.

## Git and Change Hygiene

- Inspect the working tree before and after edits.
- Preserve user-owned changes and work with overlapping edits carefully.
- Do not reset, discard, rewrite, or delete unrelated work.
- Keep diffs scoped to the acceptance criteria and required remediation.
- Do not commit, push, create a branch/PR, or modify remote state unless requested by the active workflow.
- Do not mix generated artifacts, opportunistic cleanup, package upgrades, or formatting churn into an unrelated task.
- Review `git diff --check`, the complete diff, and changed file list before handoff.

## Definition of Done

A task is complete only when:

- Acceptance criteria and relevant failure paths are satisfied.
- Behavior changes and bug fixes have appropriate tests that were observed failing before implementation.
- Focused and full applicable validation commands have passed, or every environmental limitation is disclosed.
- New and directly affected code respects the dependency matrix.
- Related architecture debt on the change path is removed.
- No new warning, secret, security regression, unbounded query, unrelated diff, or manual snapshot edit exists.
- Public contract, migration, configuration, and deployment effects are documented when applicable.
- The final report cites actual commands and distinguishes completed verification from assumptions.

## Design Risks and Mitigations

### Strict rules can inflate small tasks

Mitigation: remediation is limited to violations directly on the change path. Material scope expansion requires an explicit decision rather than silent refactoring.

### Current baseline cannot pass a zero-warning gate

Mitigation: distinguish repository baseline from task-local compliance, require directly touched projects to become clean, and move to global warnings-as-errors once debt is removed.

### A single root file can become too long

Mitigation: use a predictable table of contents and concise normative language. Split only when project-specific instructions become independently useful.

### Instructions without automation may drift

Mitigation: require architecture and quality checks as test infrastructure is introduced, and treat future CI enforcement as a follow-up task rather than claiming documentation alone enforces the design.

## Acceptance Criteria for AGENTS.md

The eventual root file must:

1. Describe the observed repository accurately.
2. Define an unambiguous dependency matrix and layer ownership.
3. State strict handling of directly touched legacy violations.
4. Define the full harness flow with stop conditions and evidence requirements.
5. Cover C#, persistence, migrations, API contracts, security, observability, testing, git hygiene, and completion criteria.
6. Include verified solution commands and distinguish standard gates from current baseline debt.
7. Avoid placeholders, vague optional language, contradictions, and rules that require nonexistent tooling without explaining the rollout.
8. Remain documentation-only for this task; no production code or behavior changes are included.
