# AGENTS.md

Business rules, the endpoint list and project-specific technical facts (database provider, the value of `[Project]`, known exceptions) live in `Requirements.md`. Read it before adding or changing a use case. This file describes how the code is written and is the same for every project that uses this architecture.

Placeholders used below: `[Project]` (project name prefix, e.g. for the `DbContext`), `[Module]` (feature folder, plural), `[Entity]` (domain entity), `[UseCaseName]`, `[Action]`, `[Property]`, `[Reason]`.

## Stack
- ASP.NET Core Web API with **Controllers**. The target framework, nullable and implicit usings are set once in `Directory.Build.props`.
- Clean Architecture + CQRS
- **No MediatR/Mediator. No AutoMapper** (manual mapping only)
- FluentValidation, EF Core, Scrutor, Serilog

## Layers (dependencies point inward only)
- `Domain`: entities, enums, domain exceptions. References nothing.
- `Application`: commands, queries, handlers, validators, mappings, interfaces (e.g. `IApplicationDbContext`). References Domain.
- `Infrastructure`: EF Core, migrations, external services. Implements Application interfaces.
- `WebAPI`: controllers, `Program.cs`. Composition root.

Handlers depend on interfaces, never on concrete `DbContext` or Infrastructure types.

## Data Access (`IApplicationDbContext`)
- Reads go through one `IQueryable<[Entity]>` property per entity on the interface.
- Writes go through explicit methods on the interface (`Add[Entity]`, `Remove[Entity]`, ...). When a new entity or write operation is needed, add the matching method to the interface and implement it in `[Project]DbContext`. Do not expose `DbSet<T>` or the `DbContext` itself.
- Database-provider details (e.g. provider error codes for unique or primary-key violations) are handled inside Infrastructure and surfaced to Application as a `bool` or a plain return value, for example through a dedicated `TrySave[Entity]Async` method returning `bool`. Application code never references provider-specific types.

## EF Core Entity Configuration
- Each persisted entity must define its EF Core mapping in `[Entity]Configuration.cs` under `Infrastructure/Persistence/Configurations` using `IEntityTypeConfiguration<[Entity]>`.
- `OnModelCreating` must apply these configurations and, when the project has seed data, call `[Project]Seed.Configure(modelBuilder)`.
- Column lengths come from the entity's constants (e.g. `[Entity].Maximum[Property]Length`), never from literal numbers.

## Migrations
- Create migrations with the EF Core CLI only, from the repository root:
  `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebAPI --output-dir Persistence/Migrations`
- Do not hand-write new migrations and do not edit a migration that has already been applied. Change the model, then add a new migration.
- Seed data lives in `[Project]Seed`. Changing it requires a new migration.
- Known exceptions to these rules (e.g. an old hand-written migration) are listed in `Requirements.md`. Do not copy their style.
- The database is migrated automatically at startup (`MigrateDatabaseAsync` in `Program.cs`).

## Application Structure
```
Application/
  Common/
    Abstractions/   (ICommandHandler, IQueryHandler, IApplicationDbContext)
    Decorators/     (ValidationDecorator, LoggingDecorator)
    Pagination/     (Pagination, PageQueryValidator)
    Responses/      (every response record: [Entity]Response, ...)
    Results/        (Result, Error, PagedResult)
  [Module]/
    [Entity]Mappings.cs        (only when a projection is shared by 2+ use cases)
    Commands/[UseCaseName]/
    Queries/[UseCaseName]/
  Shared/
    Errors/[Module]Errors.cs
  DependencyInjection.cs
```

## File Naming
- `[UseCaseName]Command.cs` / `[UseCaseName]Query.cs`
- `[UseCaseName]CommandHandler.cs` / `[UseCaseName]QueryHandler.cs`
- `[UseCaseName]CommandValidator.cs` / `[UseCaseName]QueryValidator.cs`
- `[ResponseName]Response.cs`: every response record lives in `Application/Common/Responses/` with the namespace `Application.Common.Responses`, one file per response, whether one or many use cases return it.
- `[UseCaseName]Mapping.cs` only if the use case has 2+ mappings; otherwise map inside the handler.

## Rules
- Commands change state and return `Result<T>` when there is a response payload (e.g. `Result<Guid>` for a created entity ID), or `Result` when there is no payload. Queries are read-only and return `Result<TResponse>`.
- Commands and queries are plain records. No marker interfaces.
- Handlers: `internal sealed`, primary constructor, return `Result` or `Result<T>` according to the command or query contract, always pass `CancellationToken`.
- For expected failures (NotFound, Validation, Conflict), return a failed `Result` or `Result<T>` containing an `Error`.
- Use `ValidationDecorator` to validate commands and queries that have a registered validator. Use a logging decorator for handler execution. Follow the Transactions section for transaction handling.
- DI: `Application` exposes `AddApplication()`, and `Infrastructure` exposes `AddInfrastructure()`. In `AddApplication()`, use Scrutor to register command and query handlers as scoped services under their handler interfaces. Set `publicOnly: false` in `AddClasses` to include `internal sealed` handlers.

## Mapping
- Commands: build entities through domain factory methods (`[Entity].Create(...)`), not object initializers. Use a `ToEntity` extension only when the factory call needs non-trivial argument mapping.
- Queries: use `AsNoTracking()` and project directly to the response with `Select`. Never load the entity and map it in memory.
- When the same projection is used by 2+ handlers, define it once as a static `Expression<Func<[Entity], [Entity]Response>>` in `[Module]/[Entity]Mappings.cs` (e.g. `[Entity]Mappings.ToResponse`) and pass it to `Select`. Do not duplicate the `new [Entity]Response(...)` expression.

## Validation
- Every command has a validator.
- Every query that takes an ID or is paginated has a validator. IDs are checked with `NotEmpty()` (route constraint `{id:guid}` still accepts `Guid.Empty`).
- Paginated query validators inherit from `PageQueryValidator<T>`.
- Validators mirror domain constraints using the entity's constants (`[Entity].Maximum[Property]Length`), so invalid user input is rejected before reaching the domain.
- Domain factory methods still guard their invariants with `ArgumentException` / `ArgumentOutOfRangeException`. These only fire on programming errors and end up in the global exception handler as a 500.

## Errors
- Each module has an `internal static class [Module]Errors` in `Application/Shared/Errors`, with `public static readonly Error` fields.
- Error codes use the form `[Module].[Reason]` with a plural module name, e.g. `[Module].NotFound`.
- `ErrorType` maps to HTTP status in `ResultExtensions.ToProblemDetails`: `Validation` → 400, `NotFound` → 404, `Conflict` → 409, `BadGateway` → 502, `GatewayTimeout` → 504. The error code is returned in the `code` extension of the `ProblemDetails`.

## Controllers
- No business logic. Inject handlers per action with `[FromServices]`, convert a failed `Result` with `result.ToProblemDetails(HttpContext)`.
- Request body:
  - When the body maps 1:1 to the command, bind the command record directly (e.g. `Create[Entity]Command`).
  - When the command also needs route values, define a `[Action]Request` record at the bottom of the controller file and build the command in the action.
- Responses:
  - Create command returning an ID → `201 Created` via `CreatedAtAction(nameof(Get), new { id }, id)`.
  - Command without payload → `204 No Content`.
  - Query → `200 OK` with the response.
- Every action has `[EndpointSummary]` and a `[ProducesResponseType]` for the success status and for every error status it can return (`ValidationProblemDetails` for 400, `ProblemDetails` for 404/409).
- List endpoints take `page` and `pageSize` from the query string with defaults from `Pagination`.
- Keep `WebAPI/WebAPI.http` in sync with the endpoints: add an example for every new endpoint and remove examples for deleted ones.
- API documentation, Development only: `MapOpenApi()` serves the OpenAPI document at `/openapi/v1.json`, and `MapScalarApiReference()` (package `Scalar.AspNetCore`) serves an interactive UI at `/scalar` to read and call the endpoints. Both are generated from the controllers, so `[EndpointSummary]` and `[ProducesResponseType]` are what the UI shows. Do not expose them outside Development.

## External Services
Placeholder: `[Service]` (external API name, e.g. the provider's product name).
- Application owns the contract: `I[Service]Client` in `Application/Common/Abstractions`, returning `Result<T>` with response records from `Application/Common/Responses`. Application never references `HttpClient`, HTTP status codes or the external JSON shapes.
- Each external service is one Application module named after the service, not one module per external resource:
  - use cases in `Application/[Service]/Commands/[UseCaseName]/` and `Application/[Service]/Queries/[UseCaseName]/`, named by the resource (e.g. `ListUsers`, `GetUser`, `CreatePost`);
  - responses `[Service][Resource]Response` in `Application/Common/Responses/` (e.g. `[Service]UserResponse`);
  - the service's own errors in `Application/Shared/Errors/[Service]Errors.cs`, with codes `[Service].[Reason]` (e.g. `[Service].UserNotFound`).
- Infrastructure owns the implementation in `Infrastructure/ExternalServices/[Service]/`:
  - `ExternalServicesOptions` (`Infrastructure/ExternalServices/`): settings shared by all external services, bound from the `ExternalServices` section. It holds one `TimeoutSeconds` used by every client; it is set in configuration and not validated in code. Do not add a per-service timeout.
  - `[Service]Options`: the service's own settings (its `BaseUrl`), bound from `ExternalServices:[Service]` with `BindConfiguration`. `BaseUrl` is written in configuration as an absolute URL ending with `/`; it is not validated or normalized in code. Never hard-code URLs or timeouts.
  - `[Service]Models.cs`: `internal` records matching the external JSON. They never leave Infrastructure; the client maps them manually to response records.
  - `[Service]Client`: `internal sealed` typed HttpClient registered with `AddHttpClient<I[Service]Client, [Service]Client>` in `AddInfrastructure()`, which sets `BaseAddress = new Uri([Service]Options.BaseUrl)` and `Timeout` from `ExternalServicesOptions.TimeoutSeconds`.
- The client translates every expected failure in one private `SendAsync` helper and never throws for them:
  - HTTP 404 → `ExternalServiceErrors.NotFound`
  - any other non-success status or `HttpRequestException` → `ExternalServiceErrors.Unavailable` (502)
  - `OperationCanceledException` while the caller's token is not cancelled (the timeout elapsed) → `ExternalServiceErrors.Timeout` (504)
  - `JsonException`, an empty body, or a missing expected header → `ExternalServiceErrors.InvalidResponse` (502)
  - a cancellation requested by the caller is rethrown.
  Each failure is logged as a Warning with method and URL.
- Handlers translate `ExternalServiceErrors.NotFound` to the service's own error (e.g. `[Service]Errors.UserNotFound`) and return other errors as they are.
- External list endpoints still return `PagedResult<T>`: use the external API's paging parameters and total-count header when it has them; do not fetch everything and page in memory.
- No automatic retries by default. Never retry non-idempotent requests (POST) automatically.
- External services are not added to `/health`: their outage does not make this application unhealthy.
- One controller per external service: `[Service]Controller` with the route prefix `api/[service]` (lowercase service name), holding all of that service's endpoints (e.g. `api/[service]/[resource]`). Do not split a service's endpoints across controllers per resource.
- A create call to an external API with no local read endpoint returns `201` via `Created((string?)null, value)`.

## Cross-Cutting Rules

**Global error handling**
- Unexpected exceptions are handled by a single `IExceptionHandler` registered in WebAPI that returns a uniform `ProblemDetails`.
- Never return stack traces or exception messages to the client. Log the exception server-side.
- Every failed response is logged with the exact `ProblemDetails` JSON sent to the client (`Responded {StatusCode} with {ProblemDetails}`): `ProblemDetailsLoggingFilter`, a global result filter, logs it for controller results (Warning for 4xx, Error for 5xx), and `GlobalExceptionHandler` logs it for unexpected exceptions. Successful responses are not logged this way.

**Transactions**
- One `SaveChangesAsync` per command is the default and is already atomic.
- If a command needs multiple saves/operations in one transaction, open an explicit transaction inside that handler. No global transaction decorator.

**Concurrency**
- Use a concurrency token (`RowVersion`, a `Guid` regenerated by the entity on every change) on entities that support concurrent edits.
- Include `rowVersion` in read responses and require it in update requests.
- Use the client-supplied `rowVersion` as the expected version when saving the update.
- Return a `Conflict` error (HTTP 409) if the version is stale or EF Core raises `DbUpdateConcurrencyException`.
- An operation that changes a counter or state on another entity as a side effect does not take a client `rowVersion`, but still catches `DbUpdateConcurrencyException` and returns a `Conflict` error.

**Pagination and filtering**
- Every list query returns `Result<PagedResult<T>>` (items, page, pageSize, totalCount) using `ToPageAsync`.
- Page size has a default (`Pagination.DefaultPageSize`) and a hard maximum (`Pagination.MaximumPageSize`) enforced in the query validator.
- Every list query has a deterministic order: a business field first, then `Id` as a tie-breaker.
- Client-side filtering and sorting use one shared pattern in `Application/Common/Pagination`. If it does not exist yet, design it, document it here, and only then use it. Do not add ad-hoc filter parameters to a single endpoint.

**Domain entities**
- Entities are not anemic: business rules live inside them (`[Entity].Create(...)`, `entity.Cancel()`), with private setters and a private parameterless constructor for EF Core.
- Handlers orchestrate (load, call domain methods, save). They do not contain business rules.
- Checks that need the database (an entity exists, a duplicate exists) are done in the handler before calling the domain method, and return the matching `Error`.

**Observability**
- Use **Serilog** for structured logging (inject `ILogger<T>`, no static `Log` calls in application code).
- Serilog sinks and minimum levels are configured only in the `Serilog` section of `appsettings*.json` (read with `ReadFrom.Configuration`), never in code. Logs go to the console and to daily rolling, human-readable text files `logs/log-yyyyMMdd.txt` under the app's working directory, kept for 14 days. The `logs/` folder is git-ignored. The file uses a `Serilog.Expressions` template that draws a separator block before every event whose `EventId.Name` is `RequestStarted`; `CorrelationIdMiddleware` logs that event (method, path, query string, correlation id) at the start of each request. Do not log other events with that EventId. To trace a request, search the files for its `CorrelationId` and read down from its separator. Lines from concurrent requests can interleave.
- EF Core sensitive data logging (SQL parameter values in logs and in EF exception messages) is switched by `Database:EnableSensitiveDataLogging` in `appsettings*.json` and passed to `AddInfrastructure()`. Never hard-code it. Where it is enabled, treat log files as containing personal data. The value each project uses is recorded in `Requirements.md`.
- Every request has a correlation id set by `CorrelationIdMiddleware` from `HttpContext.TraceIdentifier`. It is server-generated (an incoming header is not trusted), returned in the `X-Correlation-Id` response header, added to the log scope, and included as `correlationId` in every `ProblemDetails`.
- Expose health check endpoints (`/health`) from the start.

## Testing
- If the solution has a test project, add or update tests for every changed use case and run them.
- Always verify changes by building the solution file (`*.slnx` / `*.sln`) at the repository root with `dotnet build` and by running the affected requests in `WebAPI/WebAPI.http`, including the failure cases.

## New Use Case Checklist
1. Check `Requirements.md` for the business rules; add the new rule there if it is missing.
2. Create the folder under `[Module]/Commands` or `[Module]/Queries`.
3. Add the command or query record, its handler, and its validator (see Validation). Add a response record and a shared projection where the Mapping section requires it.
4. Add errors to `[Module]Errors.cs` if needed.
5. If the model changed, add a migration with the EF Core CLI (see Migrations).
6. Add the controller action following the Controllers section, and an example request in `WebAPI.http`. No manual DI registration needed.
7. Add the endpoint to `Requirements.md`.
8. Run `dotnet build` on the solution file at the repository root.
