# XperienceCommunity.HealthChecks - AI Coding Agent Guide

## Project Overview
This is a **NuGet package library** providing ASP.NET Core health checks for Kentico Xperience by Kentico CMS applications. It integrates with `Microsoft.AspNetCore.Diagnostics.HealthChecks` to monitor Kentico-specific application health.

**Target Frameworks:** .NET 8.0 and .NET 10.0 (multi-targeting configured in `Directory.Build.props`)  
**Kentico Dependency:** Requires `Kentico.Xperience.Core` >= 31.0.2

## Architecture Patterns

### Health Check Implementation Pattern
All Kentico health checks follow a consistent inheritance pattern:

1. **Base Class:** `BaseKenticoHealthCheck<T>` - Generic base for checks querying Kentico `IInfoProvider<T>`
   - Provides standard exception handling via `HandleException()` 
   - Implements `GetHealthCheckResult()` to respect context's `FailureStatus` (Degraded vs Unhealthy)
   - Requires derived classes to implement `GetDataForTypeAsync()` and `GetErrorData()`

2. **Simple Checks:** Implement `IHealthCheck` directly (e.g., `ApplicationInitializedHealthCheck`)
   - Use `CMSApplication.ApplicationInitialized` to check initialization state
   - Return `HealthCheckResult` with appropriate status

3. **Query-Based Checks:** Extend `BaseKenticoHealthCheck<T>` (e.g., `EventLogHealthCheck`, `WebFarmHealthCheck`)
   - Always wrap queries in `ContextUtils.ResetCurrent()` + `CMSConnectionScope(true)`
   - Use custom extension `IObjectQueryExtensions.ToListAsync()` for async querying
   - Filter out `HealthReport` source from event logs to avoid recursive health check logging

**Example Health Check Structure:**
```csharp
public sealed class EventLogHealthCheck : BaseKenticoHealthCheck<EventLogInfo>, IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        if (!CMSApplication.ApplicationInitialized.HasValue)
            return HealthCheckResult.Degraded("Application is not Initialized.");
        
        try {
            var items = await GetDataForTypeAsync(ct);
            return items.Count >= 25 
                ? GetHealthCheckResult(context, $"There are {items.Count} errors", GetErrorData(items))
                : HealthCheckResult.Healthy($"There are {items.Count} in the event log.");
        }
        catch (Exception e) { return HandleException(e); }
    }
}
```

## Key Conventions

### Dependency Injection
- **Registration:** `AddKenticoHealthChecks()` extension method in `DependencyInjection.cs` registers all checks
- **Tags:** All checks tagged with `"Kentico"` for filtering
- **Failure Status:** Configure per-check via `failureStatus` parameter (Unhealthy vs Degraded)

### Exception Handling Strategy
The `BaseKenticoHealthCheck.HandleException()` method treats certain exceptions as **non-failures**:
- `OperationCanceledException`/`TaskCanceledException` → Healthy (expected cancellation)
- `InvalidOperationException` with DataReader/Connection messages → Healthy (transient state)
- `DataClassNotFoundException`, `LinqExpressionCannotBeExecutedException` → Healthy (missing optional data)
- All other exceptions → Unhealthy

### Kentico Query Patterns
When querying Kentico data:
```csharp
ContextUtils.ResetCurrent();  // Reset Kentico context to avoid state pollution
using (new CMSConnectionScope(true))  // Ensure fresh connection
{
    var query = Provider.Get()
        .Where(...)
        .Columns(columnNames)
        .TopN(100);
    return await query.ToListAsync(cancellationToken);  // Custom extension handles retry logic
}
```

### Custom Extensions
- **`IObjectQueryExtensions.ToListAsync`:** Wraps Kentico's async queries with retry logic for `InvalidOperationException` (handles concurrent DataReader issues)

## Build & Development

### Multi-Targeting
- `Directory.Build.props` defines shared properties: `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`
- `Directory.Packages.props` centralizes package versions via `ManagePackageVersionsCentrally`
- Use version ranges for packages: `[31.0.2,)` (minimum 31.0.2, no upper bound)

### Package Configuration
- **Private Assets:** `Kentico.Xperience.Core` and `Microsoft.SourceLink.GitHub` marked as `<PrivateAssets>all</PrivateAssets>` to avoid transitive dependencies
- **Lock Files:** `RestorePackagesWithLockFile` enabled - commit `packages.lock.json` changes
- **SourceLink:** Enabled for Release builds to embed source debugging info

### Build Commands
```bash
dotnet restore                          # Restore with lock file
dotnet build -c Release --no-restore   # Build both targets
dotnet pack -c Release                 # Create NuGet package
```

### Nullable Reference Types
- **Enabled globally:** `<Nullable>enable</Nullable>` + `<WarningsAsErrors>nullable</WarningsAsErrors>`
- All methods must handle nullability correctly
- Use `[return: NotNull]` attribute where appropriate

## Testing & Validation

### Health Check Endpoint Usage
Consumers register in `Program.cs`:
```csharp
services.AddHealthChecks().AddKenticoHealthChecks();  // Add all checks
// OR selectively:
services.AddHealthChecks().AddCheck<WebFarmHealthCheck>("Web Farm Health Check");

app.UseEndpoints(endpoints => {
    endpoints.MapHealthChecks("/kentico-health", new HealthCheckOptions() {
        ResponseWriter = HealthCheckResponseWriter.WriteResponse  // Custom JSON output
    });
});
```

### Response Format
`HealthCheckResponseWriter.WriteResponse()` returns structured JSON:
```json
{
  "status": "Healthy",
  "results": {
    "Web Farm Health Check": {
      "status": "Healthy",
      "description": "All servers in the web farm are running.",
      "data": { /* check-specific data */ }
    }
  }
}
```

## Adding New Health Checks

1. **Inherit from `BaseKenticoHealthCheck<T>`** if querying Kentico data
2. **Implement required methods:**
   - `CheckHealthAsync()` - Main health check logic
   - `GetDataForTypeAsync()` - Query Kentico data with proper scoping
   - `GetErrorData()` - Return diagnostic data for failures
3. **Register in `DependencyInjection.AddKenticoHealthChecks()`**
4. **Follow naming:** `{Feature}HealthCheck.cs` in `HealthChecks/` directory

## Important Gotchas

- **Always check `CMSApplication.ApplicationInitialized`** before Kentico API calls
- **Never query Kentico outside `CMSConnectionScope`** to avoid connection leaks
- **Exclude `HealthReport` source** from EventLog queries to prevent infinite loops
- **Return Healthy for missing data** (not Unhealthy) - only infrastructure failures are Unhealthy
- **Use `TopN()` liberally** - health checks should be fast, not exhaustive
