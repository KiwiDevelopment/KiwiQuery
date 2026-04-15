# KiwiQuery

Catch hidden EF Core queries before they hit production

EF silently executed 15 queries instead of 2.
Tests passed. No warnings.

KiwiQuery fails your tests when query count unexpectedly grows.

Tired of writing the same `DbCommandInterceptor` boilerplate in every project just to check if your endpoint makes too many queries? This is for you.

```csharp
await using var guard = factory.TrackQueries<Program, AppDbContext>();
var client = guard.CreateClient();

await client.GetAsync("/api/orders");

guard.AssertCount(exact: 1);
```

## Use cases:
- Catch N+1 queries
- Prevent query regressions
- Assert EF SQL in tests

## Installation

```
dotnet add package KiwiQuery.EFCore
```

## Requirements

- .NET 5+
- EF Core 5+
- `Microsoft.AspNetCore.Mvc.Testing`

## Why

N+1 is one of those bugs that never shows up in unit tests — only in production, under load, when it's already too late. Writing a custom interceptor to catch it in integration tests is doable but tedious, and every team ends up copy-pasting the same 50 lines.

KiwiQuery wraps that into a single method call.

## Usage

```csharp
public class OrderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrderTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetOrders_ExecutesExactlyOneQuery()
    {
        await using var guard = _factory.TrackQueries<Program, AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/api/orders");

        guard.AssertCount(exact: 1);
    }
}
```

### Assertions

```csharp
guard.AssertCount(exact: 1);              // exactly 1 query
guard.AssertAtMost(3);                    // no more than 3
guard.AssertAtLeast(1);                   // at least 1
guard.AssertCount(atLeast: 1, atMost: 3); // range
guard.AssertNoQueries();                  // nothing hit the DB
```

If you just want the number:

```csharp
Console.WriteLine(guard.Count);
```

### Multiple DbContext types

Target a specific one:

```csharp
await using var guard = _factory.TrackQueries<Program, AppDbContext>();
```

Or let it find all of them:

```csharp
await using var guard = _factory.TrackQueries<Program>();
```

### Multiple requests

The counter accumulates — useful if you want to assert a budget across several calls:

```csharp
await client.GetAsync("/api/orders");
await client.GetAsync("/api/products");

guard.AssertAtMost(2);
```

## What gets counted

`SELECT`, `INSERT`, `UPDATE`, `DELETE`, `WITH` — anything that hits your actual data. PRAGMA, schema checks, connection setup are ignored. Seeding and `EnsureCreated()` on startup don't count either.

## License

MIT
