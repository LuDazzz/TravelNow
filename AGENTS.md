# TravelNow Agent Guide

## Purpose and Scope

This file is the repository-wide operating contract for coding agents working on TravelNow. It applies from the repository root to every project and file unless a more specific `AGENTS.md` adds stricter local instructions.

The goal is not merely to preserve the current project shape. The goal is to move the repository toward strict Clean Architecture while delivering small, testable, evidence-backed changes.

Rule keywords have their usual normative meaning:

- **MUST** and **MUST NOT** are mandatory.
- **SHOULD** and **SHOULD NOT** require a concrete reason to deviate.
- **MAY** is optional.

## Instruction Precedence

Apply instructions in this order:

1. Active system, tool, and user instructions.
2. The nearest applicable nested `AGENTS.md`.
3. This root `AGENTS.md`.
4. Repository documentation and established code patterns.

A nested guide may add project-specific constraints but MUST NOT silently weaken this root contract. Resolve repository-instruction conflicts by the precedence above and surface them. Active system, tool, and user instructions remain controlling even when they require a documented deviation from this guide.

Repository evidence overrides assumptions. Inspect the current files, references, tests, and configuration before deciding how the system works. Existing code is not an approved precedent when it conflicts with the target architecture in this guide.

## Repository Snapshot

TravelNow is a .NET 10 ASP.NET Core backend using EF Core, PostgreSQL, and ASP.NET Identity. The solution is `TravelNow.slnx` and currently contains:

| Project | Current role | Target role |
| --- | --- | --- |
| `TravelNow.Domain` | Entities, identity models, enums, constants, domain exceptions | Framework-independent enterprise and domain rules |
| `TravelNow.Application` | DTOs and persistence abstractions | Use cases, application models, validation, and inward-facing ports |
| `TravelNow.Infrastructure` | EF Core context/configuration, repositories, unit of work, migrations, Identity persistence | All persistence and external-system implementations |
| `TravelNow.Shared` | General helpers | Dependency-free technical primitives only |
| `TravelNow` | ASP.NET Core host, middleware, configuration | HTTP adapter and composition root only |

Important current facts:

- The repository has no automated test project yet.
- The solution builds, but the baseline contains nullable warnings and a package vulnerability warning.
- There is no repository-wide `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, or CI quality gate yet.
- PostgreSQL development infrastructure is defined in `TravelNow/docker-compose.yml`.

Treat these as migration facts, not permission to add more debt.

## Non-Negotiable Rules

- MUST preserve user-owned and unrelated working-tree changes.
- MUST inspect `git status --short` before editing and before handoff.
- MUST keep source dependencies pointing inward according to the dependency matrix below.
- MUST write a failing test first for behavior changes and bug fixes, except for the explicit non-behavior exemptions below.
- MUST validate with fresh command output before claiming success.
- MUST NOT expose secrets, credentials, tokens, private data, stack traces, SQL, or provider details to clients or non-secure output. Server diagnostics MUST redact sensitive values.
- MUST NOT run destructive Git, database, Docker, or filesystem operations without explicit authorization and verified targets.
- MUST NOT commit, push, open a PR, change remote state, add a package, or apply a migration unless the active task authorizes it.
- MUST NOT hide failures by deleting tests, weakening assertions, adding broad suppressions, or copying a known violating pattern.
- MUST stop when a missing product decision could cause a breaking contract, weaker authorization, destructive migration, or materially different behavior.

## Target Clean Architecture

Source-code dependencies point inward. Runtime control may call outward through interfaces owned by an inner layer.

```text
HTTP / external input
        |
        v
TravelNow API host -------- composition -------> TravelNow.Infrastructure
        |                                              |
        v                                              |
TravelNow.Application <------ implements ports --------+
        |
        v
TravelNow.Domain

TravelNow.Shared: dependency-free technical primitives only
```

### Dependency Matrix

An entry in "May reference" is permission, not a requirement.

| Project | May reference | MUST NOT reference or expose |
| --- | --- | --- |
| `TravelNow.Shared` | BCL and truly infrastructure-neutral packages | Any TravelNow project, business feature, EF Core, ASP.NET Core |
| `TravelNow.Domain` | BCL; exceptionally a dependency-free Shared primitive | EF Core, ASP.NET Core, Identity, Npgsql, serialization, logging, Application, Infrastructure, API |
| `TravelNow.Application` | Domain, dependency-free Shared primitives, pure application libraries | Infrastructure, API, `DbContext`, `DbSet`, `IQueryable`, `HttpContext`, SQL, provider exceptions/models |
| `TravelNow.Infrastructure` | Application, Domain, Shared, provider SDKs | Controllers, HTTP response envelopes, API presentation behavior |
| `TravelNow` | Application, Infrastructure; Shared only for host concerns | Domain decisions in controllers/middleware, direct persistence access from endpoints |

Do not add a project or package reference merely because it is convenient. A reference MUST follow ownership and dependency direction.

### Domain

`TravelNow.Domain` owns:

- Entities and aggregate behavior.
- Value objects and invariants.
- Domain services when behavior does not naturally belong to one entity.
- Domain events and business-significant exceptions.
- Business enums, policies, and constants.

Domain rules:

- MUST be usable without ASP.NET Core, EF Core, a database, a serializer, or a DI container.
- MUST NOT contain `[Key]`, `[DatabaseGenerated]`, relationship, table, column, JSON, or other persistence/transport attributes.
- MUST NOT read configuration, current HTTP user, system clock, environment variables, or external services directly.
- SHOULD enforce invariants at construction and state transitions, not only in controllers.
- SHOULD expose behavior rather than public setters when an invariant is involved.
- MUST keep collection invariants behind controlled mutation when membership has business meaning.
- MUST remain logging-independent.

### Application

`TravelNow.Application` owns:

- Use cases, commands, and queries.
- Application input/output models.
- Validation and orchestration.
- Business-context authorization decisions.
- Ports needed by use cases: persistence, clock, current user, external services, files, messages, and transactions.

Organize new behavior by feature and use case, for example:

```text
TravelNow.Application/
  Features/
    Posts/
      CreatePost/
        CreatePostCommand.cs
        CreatePostHandler.cs
        CreatePostResult.cs
        CreatePostValidator.cs
  Abstractions/
    Persistence/
    Identity/
    Time/
```

Do not introduce MediatR or another dispatcher solely to obtain this folder structure. Plain handlers or application services are acceptable when their contracts remain focused.

Application rules:

- One use case SHOULD have one clear entry point and one observable responsibility.
- Inputs and outputs MUST be transport-independent and MUST NOT use HTTP status codes or ASP.NET binding attributes.
- Ports MUST express use-case or domain needs, not generic provider capabilities.
- Ports MUST NOT return `IQueryable`, `DbSet`, provider entities, or deferred queries.
- A transaction abstraction MAY expose `SaveChangesAsync(CancellationToken)` but MUST NOT expose `DbContext`.
- Application code MUST NOT depend on `HttpContext`; use an application-owned current-user abstraction.
- Application code MUST NOT call `DateTime.Now`, `DateTimeOffset.Now`, or external time directly when time affects behavior; use an application-owned clock.
- For protected or resource-scoped use cases, authorization MUST verify resource ownership or business permission, not only authentication.
- Cancellation MUST flow from the API boundary through the use case into every I/O port.

### Infrastructure

`TravelNow.Infrastructure` owns:

- `TravelNowDbContext`, EF Core mappings, query implementations, and migrations.
- ASP.NET Identity persistence.
- Application port implementations.
- PostgreSQL-specific behavior.
- Cache, email, file, message, and external-provider adapters when introduced.
- Audit and soft-delete persistence behavior.

Infrastructure rules:

- MUST implement interfaces owned by Application or, for genuine domain services, Domain.
- MUST keep provider types and provider-specific failures out of port signatures, application/domain results, and client-facing responses.
- MUST translate expected provider failures only when a stable application/domain failure is meaningful.
- MAY let an unexpected provider exception bubble unchanged to the global exception boundary so its original diagnostic context is preserved in server logs. Do not catch, discard, or wrap it without adding meaning.
- MUST NOT contain product decisions that belong in Domain or Application.
- MUST NOT depend on API controllers, middleware, or response models.
- SHOULD keep each adapter replaceable behind a narrow port.

### API Host

`TravelNow` is the ASP.NET Core adapter and composition root. It owns:

- Controllers/endpoints and transport request/response models.
- Authentication wiring and HTTP authorization policies.
- Middleware, serialization, status codes, and OpenAPI.
- Configuration binding and startup validation.
- Calls to `AddApplicationDI` and `AddInfrastructureDI`.

Controller/endpoint rules:

- MUST bind and validate transport shape, invoke one use case, and map the result.
- MUST NOT contain business rules, EF queries, repository orchestration, or transaction management.
- MUST NOT inject or resolve `TravelNowDbContext`.
- MUST NOT return Domain entities or EF-tracked objects directly.
- MUST accept the request `CancellationToken` and pass it inward.
- SHOULD keep action code small enough that the business decision is visibly elsewhere.

### Shared

`TravelNow.Shared` is not a miscellaneous folder. A Shared type MUST be:

- Technical rather than business-specific.
- Dependency-free with respect to other TravelNow projects.
- Useful to more than one layer without reversing dependency direction.
- Stable enough to justify broad coupling.

If a helper imports a Domain constant, understands a feature, accesses HTTP state, or needs EF Core, it does not belong in Shared. Move it to its owning layer or invert the dependency.

### Dependency Injection and Composition

- Domain MUST NOT require DI registration.
- Application registrations belong in `TravelNow.Application/DependencyInjection.cs`.
- Infrastructure registrations belong in `TravelNow.Infrastructure/DependencyInjection.cs`.
- The API host composes layers in `TravelNow/Program.cs`.
- MUST NOT use service location through `IServiceProvider` inside business code.
- Service lifetimes MUST match ownership: scoped for request/database state, singleton only for thread-safe stateless services, transient for lightweight stateless objects when appropriate.
- Options that are required for safe startup SHOULD be bound and validated at startup.

### Cross-Layer Data Flow

The default request path is:

```text
HTTP request
  -> API request model and transport validation
  -> Application command/query
  -> Domain behavior and invariants
  -> Application-owned port
  -> Infrastructure implementation
  -> Application result
  -> API response model and HTTP status
```

Mapping belongs at boundaries. Do not make one model serve as HTTP request, application command, domain entity, and EF configuration surface simultaneously.

## Strict Legacy Remediation

The repository is migrating to the target model. Existing violations do not authorize new violations.

- New files and contracts MUST comply fully.
- If a task edits a violating file or consumes a violating contract on its main change path, it MUST remove the related violation in the same task.
- Remediation includes the consumers and tests necessary to preserve behavior.
- MUST NOT add compatibility wrappers that preserve the dependency leak indefinitely.
- MUST NOT suppress warnings, use `null!` without a framework-backed invariant, or add arbitrary defaults merely to make a gate green.
- MUST NOT refactor unrelated areas opportunistically.
- If correct remediation materially changes the requested scope, explain the dependency and obtain direction before continuing.
- Reports MUST distinguish task-local compliance from repository-wide compliance while baseline debt remains.

### Known Baseline Violations

As of 2026-08-06, agents MUST recognize at least these violations:

1. `TravelNow.Domain.csproj` references EF Core, ASP.NET Identity EF, Npgsql, and Newtonsoft.Json packages.
2. Domain entities use persistence-related data annotations.
3. `TravelNow.Application.csproj` references EF Core, Identity EF, Npgsql, and provider tooling.
4. `IUnitOfWork` exposes `DbContext` and uses a non-cancellable `SaveChanges` contract.
5. `IBaseRepository<T>` exposes expressions and generic query-provider concerns rather than use-case-focused ports.
6. `TravelNow.Shared` references `TravelNow.Domain` for date-format constants.
7. `ApiResponse<T>` lives in Application although it represents the current API presentation envelope.
8. The solution has no unit, integration, API contract, or architecture test project.
9. The current build reports nullable initialization warnings and `NU1903` for transitive `Microsoft.OpenApi` 2.0.0.
10. Repository-wide formatting, analyzer, package-centralization, and CI gates are not configured.

Update this list when debt is removed. Never leave a fixed item documented as current reality.

### Scope Expansion Rule

Related remediation is in scope when it is necessary to make the touched behavior and boundary correct. Repository-wide modernization is a separate task. Stop for direction when remediation would require any of the following beyond the accepted task:

- A public API break or versioning decision.
- A destructive or long-running data migration.
- A new authentication/authorization policy.
- A replacement of a shared abstraction with many unrelated consumers.
- A new external service, package family, or deployment dependency.

## Required Harness Flow

Every task follows this loop. Small tasks may use a short plan and concise updates, but MUST still produce the required evidence.

### 1. Discover

Required actions:

- Read this guide and any nested guide in scope.
- Run `git status --short`.
- Locate files with `rg --files` and usages with `rg` before broad directory traversal.
- Read the entry point, callers, consumers, project references, configuration, and existing tests.
- Inspect nearby patterns, but validate them against the target architecture.
- Identify uncommitted user changes and generated files.

Exit evidence:

- Owning layer and affected projects are known.
- The call/data path is understood.
- Relevant tests and validation commands are identified.

### 2. Frame

Required actions:

- State observable acceptance criteria.
- State non-goals where scope could expand.
- Classify risk: domain rule, public contract, authorization, persistence, migration, concurrency, external I/O, configuration, or documentation only.
- Identify known debt directly on the change path.
- Create a plan for multi-step work.

Exit evidence:

- The task has an explicit completion condition.
- Risks and scope boundaries are visible before editing.

### 3. Establish Baseline

Required actions:

- Run the narrowest relevant existing tests and build before editing when feasible.
- Record existing failures and warnings.
- Restore packages only when required.
- If verification is blocked by sandbox, network, secrets, Docker, or database availability, retry through the approved mechanism or report the precise limitation.

Exit evidence:

- Pre-existing failures are distinguishable from regressions.

### 4. Design the Change

Before implementation, decide:

- Which layer owns each decision.
- Which command/query, domain behavior, and ports are needed.
- Input, output, and error contracts.
- Validation and authorization placement.
- Transaction and consistency boundary.
- Cancellation, timeout, retry, idempotency, and observability needs.
- Whether schema or public API compatibility is affected.

Prefer the smallest complete design. Do not add abstractions or packages for hypothetical future work.

### 5. Red

For behavior changes and bug fixes:

- Add the smallest test that specifies the acceptance criterion.
- Run it and observe failure for the expected behavioral reason.
- A bug fix MUST include a regression test that reproduces the defect.

Test-first exemption:

- Documentation-only, formatting-only, comment-only, and metadata-only changes do not require a failing product test.
- Exempt changes still require structural checks, command validation, or another proportionate verification.

Do not proceed from a test that fails because of broken setup, compilation, or an unrelated dependency.

### 6. Green

- Implement the smallest coherent change that passes the focused test.
- Keep dependencies within the target matrix.
- Propagate cancellation through I/O.
- Do not weaken assertions or add production-only bypasses for tests.
- Run the focused test after each meaningful increment.

### 7. Refactor

With tests green:

- Remove duplication and boundary leaks introduced or directly exposed by the task.
- Improve names that hide business intent.
- Remove dead compatibility code made obsolete by the change.
- Keep refactoring on the direct change path.
- Re-run focused tests after refactoring.

### 8. Focused Verification

Use the smallest relevant gate first:

| Change scope | Minimum focused verification |
| --- | --- |
| Domain behavior | Domain unit test project and Domain build |
| Application use case | Application unit tests and Application build |
| EF mapping/query/repository | Infrastructure integration tests against PostgreSQL |
| Controller/auth/contract | API integration or contract tests through the host |
| Project references/packages | Affected builds, architecture checks, package audit |
| Migration | Integration coverage plus generated migration and SQL review |
| Documentation only | Required-section scan, link/command review, `git diff --check` |

No relevant test project currently exists. A behavior task MUST create the appropriate test project rather than using its absence as a reason to skip testing.

### 9. Full Verification

Run the full repository gate when implementation or configuration changes could affect compilation or behavior. Also:

- Review project dependency direction.
- Review public API and migration compatibility.
- Review package vulnerability output when dependencies change.
- Review the complete diff, not only individual files.
- Compare final warnings/failures to the recorded baseline.

### 10. Handoff

The final report MUST state:

- Behavior or documentation changed.
- Important files and boundaries changed.
- Exact commands run and whether each passed, failed, or was blocked.
- Tests added and what they prove.
- API, schema, migration, configuration, deployment, or secret impact.
- Remaining baseline debt and residual risk.

Never claim "all tests pass," "build is clean," or "fixed" without fresh evidence from the final state.

### Stop Conditions

Stop and request direction before proceeding when:

- Acceptance criteria cannot be inferred without choosing materially different product behavior.
- A public contract must break or be versioned.
- Authorization would be weakened or ownership rules are unknown.
- A migration can lose data, rewrite a large table, or needs a backfill/rollback decision.
- Completing related architecture remediation would materially exceed the accepted scope.
- A new dependency, external system, or operational permission is required but not authorized.
- Baseline verification fails unexpectedly and the failure prevents reliable regression detection.
- User-owned changes make the requested edit unsafe to isolate.

## C# and .NET Standards

### Types and Nullability

- Keep nullable reference types enabled.
- Model required and optional values truthfully at every boundary.
- Prefer constructors, `required` members, and initialized collections over suppressions.
- `null!` is allowed only when a documented framework materialization contract guarantees initialization and the mapping/test proves it; it MUST NOT be a generic warning fix.
- Avoid primitive obsession when a value has validation or business behavior that merits a value object.
- Prefer immutable request/result models where practical.
- Make classes `sealed` when inheritance is not part of their design.

### Naming and Layout

- Use file-scoped namespaces in new files.
- Keep one primary public type per file; filename and type name MUST match.
- Use `PascalCase` for types, methods, properties, and public members.
- Use `camelCase` for parameters/locals and `_camelCase` for private fields.
- Use `Id`, not `ID`, in identifiers unless matching an external contract.
- Async methods returning `Task`/`ValueTask` MUST end in `Async`.
- Names SHOULD describe business intent, not only technical mechanism such as `Manager`, `Helper`, or `Processor`.

### Async and Cancellation

- Use async APIs for I/O end to end.
- MUST NOT use `.Result`, `.Wait()`, sync-over-async, or `async void` outside true event handlers.
- Public request-scoped and I/O methods SHOULD accept `CancellationToken`, normally as the last parameter.
- Pass the token to EF Core, HTTP, file, queue, and other cancellable operations.
- Do not create a new token when the caller's token represents the operation lifetime.

### Time, Identifiers, and Determinism

- Persist instants as UTC `DateTimeOffset` unless the domain explicitly models a local date/time.
- MUST NOT use server local time for business or audit data.
- Use an application-owned clock when time changes behavior or test outcomes.
- Use an ID generator abstraction only when deterministic generation or provider choice is an actual requirement.

### General Code Quality

- Prefer simple code and explicit control flow over clever abstraction.
- Do not create generic base services/repositories without multiple proven consumers and stable shared behavior.
- Avoid boolean parameters that obscure intent; prefer named methods or options types.
- Throw or return failures at the layer that can add meaning; do not catch and rethrow unchanged.
- Comments explain rationale, invariants, compatibility constraints, or provider quirks. Do not narrate obvious assignments.
- Public contracts SHOULD have concise XML documentation when behavior is not evident from the type system.
- Remove unused usings, duplicate folder entries, dead code, and stale comments in directly touched files.

## Application and Domain Design

### Validation

- API validates transport syntax and binding concerns.
- Application validates use-case inputs and access prerequisites.
- Domain enforces invariants that must remain true regardless of caller.
- Do not rely solely on database constraints for user-facing validation, but keep database constraints as the final integrity boundary.
- Validation messages and error codes that form a public contract MUST remain stable or be versioned deliberately.

### Failures

- Expected business failures MUST use the repository's chosen consistent mechanism: typed exceptions or an explicit result type.
- Do not introduce a second failure paradigm inside the same feature without a migration decision.
- Unexpected failures propagate to the API exception boundary and are logged once with diagnostic context.
- Application/Domain failures MUST NOT carry HTTP status codes or provider exceptions.

### Ports and Transactions

- Define ports next to the layer/use case that owns the need.
- Prefer feature-specific reads/writes over expanding `IBaseRepository<T>`.
- Port methods return materialized domain/application data, not deferred provider queries.
- A use case defines the atomic consistency boundary.
- Call save/commit once per successful unit of work unless a documented intermediate boundary is required.
- Do not wrap synchronous state changes in fake async methods that only return `Task.CompletedTask`.

### Authorization and Identity

- API policies may perform coarse route-level checks.
- For protected or resource-scoped operations, Application MUST perform the required resource-level and business-context checks.
- Current identity is accessed through an Application-owned abstraction, not `IHttpContextAccessor` in use cases.
- A missing or malformed identity MUST fail safely.

## Persistence and EF Core

### Mapping

- All persistence mapping belongs in `TravelNow.Infrastructure/Configurations` or a feature-equivalent Infrastructure location.
- Use `IEntityTypeConfiguration<T>` for table, key, relationship, conversion, index, constraint, precision, and delete behavior.
- Domain types MUST NOT gain persistence attributes to make EF convenient.
- Specify maximum lengths, precision/scale, requiredness, uniqueness, and cascade behavior explicitly.
- Model configuration and database constraints MUST agree with Domain/Application validation.

### Queries

- Read-only queries SHOULD use `AsNoTracking()`.
- Project only required columns into application result/read models.
- Every potentially growing collection query MUST be bounded.
- Externally exposed list queries MUST use pagination. Internal bulk processing MUST use explicit chunking or streaming rather than an unbounded materialization.
- Define stable ordering before pagination.
- Avoid N+1 access, lazy-loading surprises, accidental client evaluation, and indiscriminate `Include` graphs.
- Use split/single-query behavior deliberately for large relationship graphs and cover the choice with an integration test.
- Pass `CancellationToken` to async EF operations.
- Do not expose `IQueryable` outside Infrastructure.

### Commands and Concurrency

- Load only state required to enforce invariants.
- Keep transactions short and free of avoidable external network calls.
- Define optimistic concurrency behavior when simultaneous writes can lose data.
- Map unique/foreign-key/concurrency failures to stable failures only when the application can act on them.
- Audit and soft-delete behavior MUST be centralized and integration-tested.
- `IgnoreQueryFilters()` requires an explicit use case, authorization review, and a test proving deleted data exposure is intentional.

### Repository Guidance

- Do not expand the current generic repository as the default design.
- New persistence ports SHOULD represent concrete operations required by a use case.
- Infrastructure may reuse private query helpers, but provider mechanics stay inside Infrastructure.
- Do not mock `DbSet` or LINQ provider behavior as proof that a PostgreSQL query works.

## Migrations and Database Safety

The repository does not currently contain a .NET tool manifest, and `dotnet-ef` may not be installed in the agent environment. Before creating a migration, run:

```powershell
dotnet ef --version
```

If the command is unavailable, do not install an unpinned global tool silently. Propose an authorized repository-local tool manifest with a version compatible with the solution's EF Core packages, or use the project team's approved setup.

Generate migrations from the repository root with an explicit project and startup project:

```powershell
dotnet ef migrations add <MigrationName> `
  --project TravelNow.Infrastructure/TravelNow.Infrastructure.csproj `
  --startup-project TravelNow/TravelNow.csproj
```

Migration rules:

- A model/schema change MUST include the generated migration and snapshot in the same change unless the task explicitly stages them separately.
- MUST NOT hand-edit `TravelNowDbContextModelSnapshot.cs`.
- A generated migration MAY be adjusted only for a documented data-safe reason; regenerate or otherwise verify snapshot consistency afterward.
- Review operations for drops, renames, narrowing types, nullability changes, defaults, backfills, indexes, locks, and table rewrites.
- Prefer explicit rename operations over drop-and-create when preserving data.
- Destructive or long-running changes require a rollout, backfill, rollback, and operational approval plan.
- Generate/review SQL for non-trivial or production-bound changes.
- MUST NOT run `dotnet ef database update` against a shared or production database without explicit authorization and a verified connection target.
- MUST NOT enable automatic production migrations at startup without an approved deployment design.
- Do not put credentials or environment-specific connection strings in migrations or committed settings.

## API and Contract Rules

- Use dedicated request and response models; prevent over-posting.
- Keep the public success/error envelope stable until an explicit contract migration moves or versions it.
- Map successful creation to an intentional `201` and location when the resource contract supports it.
- Use deliberate status codes for validation, authentication, authorization, not-found, conflict, and concurrency failures.
- Do not convert every failure to `200` merely because an envelope contains `Succeeded = false`.
- A change to route, verb, status, field name/type/nullability, enum representation, pagination, error code, or authorization is a contract change.
- Contract changes require compatibility analysis, tests, OpenAPI review, and versioning/migration when consumers may break.
- Unexpected 500 responses MUST use a generic client message and correlation/trace identifier when available.
- Never return raw exception messages, stack traces, SQL, internal paths, or provider details.
- Keep controllers free of persistence and product decisions.

## Security

### Identity and Authorization

- Authentication establishes identity; it does not prove permission.
- Apply least privilege and default-deny behavior for protected operations.
- Check ownership/tenant/resource access in Application for identifier-based operations to prevent IDOR.
- Do not trust user IDs, role names, ownership fields, or audit fields supplied by clients.
- Administrative bypasses MUST be explicit, authorized, logged appropriately, and tested.

### Input and Output

- Validate lengths, ranges, formats, collection sizes, file properties, and allowed values before expensive work.
- Use parameterized EF/provider APIs; never concatenate untrusted input into SQL.
- For URL/file features, consider SSRF, path traversal, content type, size, and storage authorization.
- Avoid reflecting untrusted input into logs or error messages without sanitization.

### Secrets and Sensitive Data

- Never commit connection strings with credentials, passwords, tokens, signing keys, or third-party secrets.
- Use environment variables, ASP.NET user secrets for local development, or an approved secret store.
- Do not print secrets in terminal output, test output, exceptions, logs, screenshots, or PR descriptions.
- Treat access tokens, refresh tokens, authentication cookies, and personal data as sensitive.
- Redact or omit sensitive values from structured logs.

### Dependency Security

- Adding or upgrading a package requires justification, license/maintenance consideration, and vulnerability review.
- Prefer framework/BCL capabilities over a new dependency when they are sufficient.
- Package vulnerabilities in a directly affected dependency path MUST be resolved or explicitly escalated; do not silently accept them.

## Logging and Observability

- Use structured logging templates and named properties; do not build messages by string concatenation.
- Include operation, stable identifiers, and trace context needed for diagnosis without logging sensitive payloads.
- Log unexpected exceptions once at the boundary responsible for handling them.
- Avoid duplicate exception logs at repository, application, middleware, and controller layers.
- Use appropriate levels: debug for diagnostic detail, information for meaningful lifecycle events, warning for recoverable abnormal conditions, error for failed operations.
- Do not add high-cardinality or sensitive labels to metrics.
- Preserve ASP.NET trace/correlation context across outbound calls when supported.
- Domain remains free of logging dependencies.

## External I/O and Resilience

- External clients live in Infrastructure behind Application-owned ports.
- Use managed client factories/lifetimes; do not construct a new `HttpClient` per request.
- Set explicit timeouts and propagate cancellation.
- Retry only transient failures and only when the operation is safe to repeat.
- Do not retry validation, authentication, authorization, or deterministic business failures.
- Define idempotency for commands that may be retried by clients, queues, or infrastructure.
- Bound payload sizes and collection results.
- Unit tests MUST NOT call live external systems; integration tests use controlled test doubles, containers, or sandboxes.

## Testing Strategy

### Test Matrix

| Test type | Required coverage | Must avoid |
| --- | --- | --- |
| Domain unit | Invariants, value objects, entity transitions, domain services/events | Database, ASP.NET host, network, mocks of domain objects |
| Application unit | Use-case orchestration, validation, authorization, port calls, failure paths | EF Core/provider types, live I/O |
| Infrastructure integration | EF mappings, PostgreSQL queries/constraints, transactions, audit, soft delete, port adapters | EF InMemory as relational proof, mocked `DbSet` |
| API integration/contract | Routing, binding, serialization, authn/authz, status codes, envelopes | Controller-only tests that bypass host behavior when contract is the concern |
| Architecture | Project/type dependency matrix and forbidden references | Manual inspection as the only enforcement once tests exist |

### Test Project Layout

Create only the projects needed by delivered behavior, following this target layout:

```text
tests/
  TravelNow.Domain.UnitTests/
  TravelNow.Application.UnitTests/
  TravelNow.Infrastructure.IntegrationTests/
  TravelNow.Api.IntegrationTests/
  TravelNow.ArchitectureTests/
```

Add created test projects to `TravelNow.slnx` and keep their dependencies pointing toward the project under test plus test-only tooling.

### Test Quality Rules

- Test observable behavior and stable contracts, not private implementation details.
- Name tests by behavior, for example `HandleAsync_WhenUserDoesNotOwnPost_ReturnsForbidden`.
- A test MUST be deterministic, isolated, order-independent, and parallel-safe unless explicitly grouped.
- Control clock, current user, IDs, and external responses through explicit boundaries.
- Integration fixtures own database/container setup and cleanup.
- Use unique test data; avoid shared mutable seed state.
- A test MUST fail for the intended reason before implementation and pass afterward.
- Cover happy path, relevant boundary values, authorization, cancellation, conflicts, and failure behavior in proportion to risk.
- Do not chase a numeric coverage target while leaving important decisions untested.
- Do not remove or weaken an existing assertion unless the accepted behavior changed and the reason is documented.

## Standard Commands

Run commands from the repository root unless stated otherwise.

### Prerequisites and Restore

```powershell
dotnet --version
dotnet restore TravelNow.slnx
```

Restore may require approved network access. Do not repeatedly restore during fast feedback when assets are current.

### Fast Feedback

Build only the affected project when project references and restored assets allow it:

```powershell
dotnet build TravelNow.Domain/TravelNow.Domain.csproj --no-restore --nologo
dotnet build TravelNow.Application/TravelNow.Application.csproj --no-restore --nologo
dotnet build TravelNow.Infrastructure/TravelNow.Infrastructure.csproj --no-restore --nologo
dotnet build TravelNow/TravelNow.csproj --no-restore --nologo
```

After test projects exist, run `dotnet test` against the concrete test project and use `--filter` with the concrete test name when narrower feedback is useful. Never report an example command as executed evidence.

### Full Gate

```powershell
dotnet restore TravelNow.slnx
dotnet format TravelNow.slnx --verify-no-changes --no-restore
dotnet build TravelNow.slnx --no-restore --nologo
dotnet test TravelNow.slnx --no-build --nologo
```

Interpretation:

- Restore MUST succeed when dependencies or project files changed.
- Formatting failures in touched C# files MUST be fixed. Do not mass-format unrelated legacy files without scope approval.
- Build MUST succeed with no new warnings.
- Tests MUST include relevant test projects; exit code 0 with no test projects is not evidence that behavior is covered.

The current full formatting gate has known failures in legacy C# files. A pre-existing full-gate failure outside the accepted change path does not by itself block task-local completion only when all of these are true:

- The failure was recorded during baseline verification and is unchanged in the final run.
- No failing file or owning project was directly modified by the task.
- A focused formatter/build/test check for every touched file or project passes where such a check exists.
- The handoff identifies the exact failing command and files and does not claim that the full gate passed.

If the task touches a failing file/project or changes the failure output, the related failure MUST be fixed. This baseline exception never permits a new failure, a weaker check, or a hidden regression.

### Conditional Gates

When package references change:

```powershell
dotnet list TravelNow.slnx package --vulnerable --include-transitive
```

When Docker Compose changes and Docker is available:

```powershell
docker compose -f TravelNow/docker-compose.yml config --quiet
```

When EF model/schema changes:

- Run relevant Infrastructure integration tests.
- Inspect the generated migration and snapshot diff.
- Generate and review SQL for destructive or operationally significant changes.

When dependencies or architecture boundaries change:

- Run architecture tests once present.
- Until then, inspect every changed `.csproj` and use `rg` to check forbidden namespaces/types.

### Warning Policy

- New warnings are forbidden.
- Warnings in directly touched files MUST be fixed, not suppressed.
- A directly modified project MUST build warning-free. If an unchanged warning is emitted only from an unmodified transitive project, list it precisely and do not add to it.
- Dependency changes MUST not introduce unresolved vulnerability warnings.
- Existing unrelated baseline warnings MUST be reported until removed by an authorized task.
- Once the solution reaches zero warnings, enable and preserve warnings-as-errors for the full build.

## Git and Change Hygiene

### Before Editing

- Run `git status --short` and identify user-owned changes.
- Inspect relevant diffs before editing a file already changed by the user.
- Do not assume an untracked file is disposable.

### While Editing

- Use the repository's existing style unless it violates this guide.
- Keep changes scoped to acceptance criteria and required related remediation.
- Do not mix package upgrades, generated artifacts, broad formatting, or unrelated cleanup into the task.
- Use structured parsers/tooling for structured files and EF tooling for migrations.
- Do not overwrite concurrent user changes; re-read before applying overlapping edits.

### Before Handoff

Run commands that include both staged and unstaged changes:

```powershell
git diff HEAD --check
git status --short
git diff HEAD --stat
git diff HEAD
```

These commands do not show untracked-file contents. For every `??` entry, inspect the content directly before staging, or stage only the task-owned file and inspect it through the cached diff.

Review the exact staged scope separately before committing:

```powershell
git diff --cached --check
git diff --cached
```

When the task includes commits, review the cumulative branch scope against the verified base before publishing:

```powershell
git log --oneline origin/main..HEAD
git diff --check origin/main...HEAD
git diff --name-status origin/main...HEAD
git diff origin/main...HEAD
```

Rules:

- MUST NOT use `git reset --hard`, destructive checkout, clean, forced push, history rewrite, or deletion to solve ordinary conflicts.
- MUST NOT revert user changes unless explicitly requested.
- Commit only files intentionally included in the task.
- Commit messages SHOULD be imperative and describe the delivered behavior or documentation.
- Push/PR only when authorized; verify branch, base, remote, and diff first.

## Definition of Done

A task is complete only when all applicable statements are true:

### Behavior and Scope

- Acceptance criteria are satisfied, including relevant failure paths.
- Non-goals remain unchanged.
- No unrelated behavior, dependency, schema, or formatting churn was introduced.

### Architecture

- New and changed code follows the dependency matrix.
- Business decisions are in Domain/Application, not API/Infrastructure.
- Directly touched legacy violations are removed or an approved scope decision is documented.
- Ports do not leak infrastructure/provider/HTTP types.

### Tests and Verification

- A new test was observed failing first for each behavior change or bug fix.
- Focused tests pass.
- Applicable integration, API contract, migration, architecture, and package checks pass.
- Full build/test gates were run, or the exact environmental blocker and narrower evidence are reported.
- No new warnings were introduced.

### Data, API, and Security

- Authorization and ownership are enforced server-side.
- No secret or sensitive data appears in code, logs, output, tests, or diffs.
- Public contract changes are compatible, versioned, or explicitly approved.
- Migration/data effects are reviewed, reversible where required, and not applied without authorization.
- Error responses are sanitized and diagnostics remain available in server logs.

### Change Quality

- `git diff --check` passes.
- The complete changed-file list and diff were reviewed.
- Generated files are intentional and consistent.
- Documentation and operational notes are updated where the change affects usage or deployment.
- Handoff contains actual verification evidence and residual risks.

## Handoff Template

Use a concise version of this structure:

```markdown
Summary:
- <observable change>
- <important architecture or contract decision>

Verification:
- `<exact command>` - PASS/FAIL/BLOCKED (<important result>)

Impact:
- API: none/<details>
- Database/migration: none/<details>
- Configuration/deployment: none/<details>

Residual risk:
- none/<baseline warning, unrun check, or follow-up>
```

Do not include empty ceremony. Omit sections that genuinely do not apply, but never omit failed or blocked verification.

## Maintaining This Guide

- Update repository facts, commands, known debt, and project ownership when they change.
- Remove known-debt entries in the same change that fixes them.
- Prefer automated architecture/analyzer/test/CI enforcement over adding more prose when a rule can be checked reliably.
- Add a nested `AGENTS.md` only when a subtree has a distinct workflow, tooling, generated-code policy, or stricter boundary.
- A nested guide MUST state its scope and inherit this root guide explicitly.
- Avoid duplicating the same rule in multiple guides; keep shared rules here and project-specific rules near their files.
- Review this guide when introducing a new project, persistence technology, external adapter, test strategy, or deployment model.
