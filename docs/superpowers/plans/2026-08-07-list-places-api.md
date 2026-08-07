# List Places API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with verification checkpoints.

**Goal:** Deliver a paginated `GET /api/places` read API whose dependencies follow the TravelNow Clean Architecture boundaries.

**Architecture:** The API host owns HTTP query/response models and maps them to an Application query. Application owns the use case, result models, validation, and a narrow materialized read port. Infrastructure implements that port with an EF Core `AsNoTracking` projection, stable ordering, bounded pagination, and cancellation. Existing generic repository and `ApiResponse<T>` leaks remain outside this slice unless the new path would consume them.

**Tech Stack:** .NET 10, ASP.NET Core MVC, EF Core/Npgsql, xUnit, nullable reference types.

---

### Task 1: Create the Application test project and specify the use case

**Files:**
- Create: `tests/TravelNow.Application.UnitTests/TravelNow.Application.UnitTests.csproj`
- Create: `tests/TravelNow.Application.UnitTests/Features/Places/ListPlaces/ListPlacesHandlerTests.cs`
- Modify: `TravelNow.slnx`

- [x] **Step 1: Add the failing handler test**

  Define an in-memory fake `IPlaceReadPort` in the test file and assert that a valid query returns the requested page metadata and forwards the filters. The test should reference the Application project and compile against the intended `ListPlacesQuery`, `ListPlacesHandler`, `PlaceListResult`, `PlaceListItem`, and `IPlaceReadPort` contracts.

- [x] **Step 2: Run the focused test and verify the expected RED failure**

  Run `dotnet test tests/TravelNow.Application.UnitTests/TravelNow.Application.UnitTests.csproj --no-restore`. Expected result: compilation fails because the new Application contracts and handler do not yet exist; no production implementation is written before this failure is observed.

- [x] **Step 3: Add the minimal Application contracts and handler**

  Create `TravelNow.Application/Features/Places/ListPlaces/ListPlacesQuery.cs` as an immutable query with `Page`, `PageSize`, nullable `ProvinceId`, and nullable `Keyword`; `ListPlacesResult.cs` with materialized `PlaceListItem` records and `TotalCount`; `IPlaceReadPort.cs` under `TravelNow.Application/Abstractions/Persistence/Places` returning `Task<IReadOnlyList<PlaceListItem>>` plus total count through a focused result; and `ListPlacesHandler.cs` that validates page bounds, caps page size at 100, normalizes blank keywords to null, and calls the port once with the caller's cancellation token. Keep all types transport-independent and free of EF/HTTP references.

- [x] **Step 4: Run the focused test and verify GREEN**

  Run the same `dotnet test` command. Expected result: the handler test passes.

### Task 2: Implement the Infrastructure read adapter

**Files:**
- Create: `TravelNow.Infrastructure/Features/Places/PlaceReadPort.cs`
- Modify: `TravelNow.Infrastructure/DependencyInjection.cs`

- [x] **Step 1: Add an Infrastructure adapter test seam**

  Extend the Application test only with contract-level assertions; do not mock `DbSet` or LINQ. The EF behavior is verified through the existing `TravelNowDbContext` against PostgreSQL when available, otherwise the focused application test remains the deterministic gate and the blocked integration requirement is reported.

- [x] **Step 2: Implement the EF projection**

  Implement `IPlaceReadPort` using `TravelNowDbContext.Places.AsNoTracking()`. Apply optional `ProvinceId` and case-insensitive keyword filtering supported by PostgreSQL, order by `Name` then `Id`, count before pagination, and project only `Id`, `Name`, `ProvinceId`, `Province.Name`, and `Location`. Use `Skip`/`Take` after clamping values and pass `CancellationToken` to both count and list operations. Return an Application-owned materialized result; no `IQueryable` crosses Infrastructure.

- [x] **Step 3: Register the adapter**

  Register `IPlaceReadPort` to `PlaceReadPort` as scoped in `TravelNow.Infrastructure/DependencyInjection.cs`. Do not add a generic repository method or expose the DbContext to the API.

- [x] **Step 4: Build the affected projects**

  Run `dotnet build TravelNow.Application/TravelNow.Application.csproj --no-restore` and `dotnet build TravelNow.Infrastructure/TravelNow.Infrastructure.csproj --no-restore`. Expected result: both exit 0 with only the recorded baseline warnings.

### Task 3: Add the HTTP adapter and contract mapping

**Files:**
- Create: `TravelNow/Controllers/PlacesController.cs`
- Create: `TravelNow/Models/Places/ListPlacesRequest.cs`
- Create: `TravelNow/Models/Places/ListPlacesResponse.cs`
- Modify: `TravelNow/Program.cs` only if controller discovery/serialization needs composition changes

- [x] **Step 1: Add the controller contract test**

  Add an API-level unit test only if the existing test setup supports it; otherwise cover the controller through a host integration test after implementation. The acceptance assertion is that `GET /api/places` binds query values, invokes the handler with the request cancellation token, and returns a dedicated response model rather than Domain or EF entities.

- [x] **Step 2: Implement transport models and controller**

  Bind query parameters with explicit range validation (`page` 1..`int.MaxValue`, `pageSize` 1..100), create the Application query, invoke the handler once, and map the result to a JSON response with `items`, `page`, `pageSize`, `totalCount`, and `totalPages`. Keep HTTP status selection and binding concerns in the controller; do not use `ApiResponse<T>` from Application.

- [x] **Step 3: Run API build and focused tests**

  Run `dotnet test tests/TravelNow.Application.UnitTests/TravelNow.Application.UnitTests.csproj --no-restore` and `dotnet build TravelNow/TravelNow.csproj --no-restore`. Expected result: tests pass and the API builds.

### Task 4: Harden the API exception boundary

**Files:**
- Modify: `TravelNow/Middlewares/ExceptionHandlingMiddleware.cs`
- Modify: `tests/TravelNow.Api.IntegrationTests/Controllers/PlacesControllerTests.cs`

- [x] **Step 1: Add the failing exception disclosure regression test**

  Configure the test port to throw an exception containing provider details and assert that the HTTP response is `500`, includes a trace id, and excludes the exception text.

- [x] **Step 2: Replace the legacy envelope with safe ProblemDetails output**

  Map expected domain/security failures to stable generic details, log unexpected failures with a structured trace id, and never serialize exception messages to the client.

- [x] **Step 3: Run API tests and verify GREEN**

  Run `dotnet test tests/TravelNow.Api.IntegrationTests/TravelNow.Api.IntegrationTests.csproj --no-restore`. Expected result: 3 tests pass.

### Task 5: Full verification and handoff

**Files:**
- Modify: `docs/superpowers/plans/2026-08-07-list-places-api.md` to mark completed steps only after evidence

- [x] **Step 1: Run the repository gate**

  Run `dotnet test TravelNow.slnx --no-restore` followed by `dotnet build TravelNow.slnx --no-restore` and `git diff --check`. Record exit codes and distinguish baseline warnings from new failures.

- [x] **Step 2: Review architecture and contract changes**

  Inspect `git diff`, verify no API/Application dependency on `DbContext`/`IQueryable`, verify cancellation reaches the port, and confirm no migration, secret, or configuration changes were introduced.

- [ ] **Step 3: Commit the implementation**

  Run `git status --short`, stage only the plan, Application, Infrastructure, API, and test files, then commit with `git commit -m "feat: add paginated places api"`.

- [ ] **Step 4: Push and create the PR through GitHub MCP**

  Push the current branch with the approved Git operation. Use the connected GitHub MCP connector to create a draft PR targeting the repository's default branch, with a summary of the Places API, tests, baseline warnings, and explicit note that no migration was generated. Do not install or invoke `gh` CLI.
