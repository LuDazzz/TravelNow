# AGENTS.md Clean Architecture Harness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a repository-root `AGENTS.md` that enforces strict Clean Architecture boundaries and a complete, evidence-based harness flow for future TravelNow coding tasks.

**Architecture:** Keep one authoritative instruction file at the repository root. The file documents the target dependency direction, layer ownership, strict remediation of directly touched legacy violations, task lifecycle, engineering rules, validation gates, and Definition of Done without modifying production behavior.

**Tech Stack:** Markdown, .NET 10, ASP.NET Core, EF Core, PostgreSQL, PowerShell, Git

---

## File Map

- Create: `AGENTS.md` - authoritative repository-wide instructions for coding agents.
- Create: `docs/superpowers/plans/2026-08-06-agents-clean-architecture-harness.md` - this execution plan.
- Reference: `docs/superpowers/specs/2026-08-06-agents-clean-architecture-harness-design.md` - approved design and acceptance criteria; do not modify during implementation unless a contradiction is discovered.

No production source, project file, package reference, migration, configuration, or test project is changed by this plan.

### Task 1: Establish the Documentation Baseline

**Files:**
- Inspect: `TravelNow.slnx`
- Inspect: `TravelNow/TravelNow.csproj`
- Inspect: `TravelNow.Application/TravelNow.Application.csproj`
- Inspect: `TravelNow.Domain/TravelNow.Domain.csproj`
- Inspect: `TravelNow.Infrastructure/TravelNow.Infrastructure.csproj`
- Inspect: `TravelNow.Shared/TravelNow.Shared.csproj`
- Inspect: `docs/superpowers/specs/2026-08-06-agents-clean-architecture-harness-design.md`

- [x] **Step 1: Confirm the branch and working tree**

Run:

```powershell
git branch --show-current
git status --short
```

Expected: branch is `docs/agents-clean-architecture-harness`; only the implementation plan is untracked before its checkpoint commit.

- [x] **Step 2: Confirm the solution baseline**

Run:

```powershell
dotnet build TravelNow.slnx --no-restore --nologo
```

Expected: exit code 0. The existing baseline may report the documented nullable and package vulnerability warnings. Record the exact warning count; do not treat existing warnings as introduced by the documentation change.

- [x] **Step 3: Confirm no root instruction file exists**

Run:

```powershell
Test-Path -LiteralPath AGENTS.md
```

Expected: `False`.

- [x] **Step 4: Checkpoint the implementation plan**

Run:

```powershell
git add docs/superpowers/plans/2026-08-06-agents-clean-architecture-harness.md
git commit -m "docs: plan agent harness instructions"
```

Expected: one commit containing only the implementation plan.

### Task 2: Create the Root Agent Operating Contract

**Files:**
- Create: `AGENTS.md`
- Reference: `docs/superpowers/specs/2026-08-06-agents-clean-architecture-harness-design.md`

- [ ] **Step 1: Create the document header and instruction semantics**

Add these top-level sections in this order:

```markdown
# TravelNow Agent Guide

## Purpose and Scope
## Instruction Precedence
## Repository Snapshot
## Non-Negotiable Rules
```

The content must state that the file applies to the entire repository, narrower instructions may add constraints but cannot weaken root rules, repository evidence overrides assumptions, existing code is not precedent when it violates the target architecture, and agents must preserve user-owned working-tree changes.

- [ ] **Step 2: Add the target architecture contract**

Add these sections:

```markdown
## Target Clean Architecture
### Dependency Matrix
### Domain
### Application
### Infrastructure
### API Host
### Shared
### Dependency Injection and Composition
### Cross-Layer Data Flow
```

Include the exact project ownership and permitted dependency direction from the approved spec. Explicitly prohibit EF Core, ASP.NET Core, Identity, Npgsql, serialization, logging, `DbContext`, `DbSet`, `IQueryable`, `HttpContext`, SQL, and infrastructure models from the inner layers where applicable. State that API controllers perform transport mapping and invoke one use case, while Infrastructure implements Application-owned ports.

- [ ] **Step 3: Add strict legacy remediation rules**

Add:

```markdown
## Strict Legacy Remediation
### Known Baseline Violations
### Scope Expansion Rule
```

List each observed violation from the spec. Require new code to comply, directly touched violations to be removed, tests and consumers needed for remediation to remain in the same task, and material scope expansion to be raised rather than hidden. Prohibit suppressions, null-forgiving workarounds, copied anti-patterns, and false repository-wide clean claims.

- [ ] **Step 4: Add the complete harness lifecycle**

Add:

```markdown
## Required Harness Flow
### 1. Discover
### 2. Frame
### 3. Establish Baseline
### 4. Design the Change
### 5. Red
### 6. Green
### 7. Refactor
### 8. Focused Verification
### 9. Full Verification
### 10. Handoff
### Stop Conditions
```

For every stage, include concrete required actions and exit evidence. Require test-first work for behavior changes and bug fixes, with explicit exemptions for documentation-only, formatting-only, and metadata-only tasks. Require agents to stop before ambiguous breaking contracts, security weakening, destructive data operations, or materially different behavior.

- [ ] **Step 5: Add engineering rules by concern**

Add:

```markdown
## C# and .NET Standards
## Application and Domain Design
## Persistence and EF Core
## Migrations and Database Safety
## API and Contract Rules
## Security
## Logging and Observability
## External I/O and Resilience
```

Cover nullable correctness, async naming, cancellation propagation, UTC time, ownership-based naming, minimal abstractions, options validation, application-owned ports, transaction boundaries, query projection, `AsNoTracking`, pagination, N+1 avoidance, explicit EF mapping, migration generation/review, API compatibility, sanitized failures, authorization and ownership checks, secrets/PII, structured logging, timeouts, retries, and idempotency.

- [ ] **Step 6: Add test strategy and quality gates**

Add:

```markdown
## Testing Strategy
### Test Matrix
### Test Project Layout
### Test Quality Rules
## Standard Commands
### Fast Feedback
### Full Gate
### Conditional Gates
### Warning Policy
```

Define Domain/Application unit tests, Infrastructure PostgreSQL integration tests, API host contract tests, and architecture tests. Include the recommended `tests/` project layout. Require deterministic and isolated tests. Include these standard full-gate commands:

```powershell
dotnet restore TravelNow.slnx
dotnet format TravelNow.slnx --verify-no-changes --no-restore
dotnet build TravelNow.slnx --no-restore
dotnet test TravelNow.slnx --no-build
```

State that agents first run narrow commands and then the full gate. Require architecture, migration, and package vulnerability checks when relevant. No new warnings are allowed; directly modified projects and directly touched warnings must become clean; unrelated baseline warnings must be disclosed until the repository reaches global warnings-as-errors.

- [ ] **Step 7: Add change hygiene and completion evidence**

Add:

```markdown
## Git and Change Hygiene
## Definition of Done
## Handoff Template
## Maintaining This Guide
```

Require working-tree inspection, preservation of user changes, scoped diffs, no destructive Git operations, no unrequested remote mutations, `git diff --check`, complete diff review, acceptance criteria, test evidence, architecture compliance, migration/contract disclosure, and a concise handoff with commands and residual risks. State when nested `AGENTS.md` files are justified and require them to preserve the root contract.

### Task 3: Validate the Instruction File

**Files:**
- Validate: `AGENTS.md`
- Validate: `docs/superpowers/specs/2026-08-06-agents-clean-architecture-harness-design.md`

- [ ] **Step 1: Check required section coverage**

Run:

```powershell
$required = @(
  'Purpose and Scope', 'Target Clean Architecture', 'Strict Legacy Remediation',
  'Required Harness Flow', 'C# and .NET Standards', 'Persistence and EF Core',
  'Migrations and Database Safety', 'API and Contract Rules', 'Security',
  'Testing Strategy', 'Standard Commands', 'Git and Change Hygiene',
  'Definition of Done', 'Handoff Template'
)
$content = Get-Content -Raw AGENTS.md
$missing = $required | Where-Object { $content -notmatch [regex]::Escape($_) }
if ($missing) { throw "Missing AGENTS.md sections: $($missing -join ', ')" }
```

Expected: exit code 0 with no output.

- [ ] **Step 2: Scan for incomplete language and formatting errors**

Run:

```powershell
Select-String -Path AGENTS.md -Pattern 'TBD|TODO|FIXME|PLACEHOLDER|implement later' -CaseSensitive:$false
git diff --check
```

Expected: no placeholder matches and no whitespace errors.

- [ ] **Step 3: Verify standard command syntax against the current repository**

Run:

```powershell
dotnet format TravelNow.slnx --verify-no-changes --no-restore
dotnet build TravelNow.slnx --no-restore --nologo
dotnet test TravelNow.slnx --no-build --nologo
```

Expected: each command is accepted by the installed SDK. Record current formatting, warning, and test-inventory results honestly; documentation changes must not be reported as repairing baseline production issues.

- [ ] **Step 4: Review the full diff against the spec**

Run:

```powershell
git diff -- AGENTS.md
git status --short
```

Expected: `AGENTS.md` is the only uncommitted implementation file, and every acceptance criterion in the approved spec maps to a concrete rule.

### Task 4: Commit and Publish the Documentation Change

**Files:**
- Commit: `AGENTS.md`

- [ ] **Step 1: Commit the root guide intentionally**

Run:

```powershell
git add AGENTS.md
git commit -m "docs: add clean architecture agent guide"
```

Expected: a focused commit containing only `AGENTS.md`.

- [ ] **Step 2: Verify branch scope before publishing**

Run:

```powershell
git status --short
git log --oneline origin/main..HEAD
git diff --stat origin/main...HEAD
```

Expected: clean working tree and only the design spec, implementation plan, and root guide in the branch diff.

- [ ] **Step 3: Push and open a draft pull request**

Use the `github:yeet` workflow to push `docs/agents-clean-architecture-harness` and create a draft PR targeting `main`. The PR body must summarize the architecture contract, harness lifecycle, quality gates, known baseline debt, and exact verification results. Do not claim the existing production warnings or vulnerability were fixed.
