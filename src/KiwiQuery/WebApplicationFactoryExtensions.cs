using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KiwiQuery;

public static class WebApplicationFactoryExtensions
{
    /// <summary>
    /// Tracks all EF Core SQL queries executed during the test.
    /// Specify TContext to target a specific DbContext.
    /// </summary>
    public static QueryGuard TrackQueries<TEntryPoint, TContext>(
        this WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class
        where TContext : DbContext
    {
        var interceptor = new QueryInterceptor();

        var trackedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TContext>));

                if (descriptor is null)
                    return;

                services.Remove(descriptor);
                services.AddScoped<DbContextOptions<TContext>>(sp =>
                {
                    var original = (DbContextOptions<TContext>)descriptor.ImplementationFactory!(sp);
                    return new DbContextOptionsBuilder<TContext>(original)
                        .AddInterceptors(interceptor)
                        .Options;
                });
            });
        });

        // warm up: trigger app startup + seeding before the guard is returned
        trackedFactory.CreateClient().Dispose();
        interceptor.Reset();

        return new QueryGuard(interceptor, () => trackedFactory.Dispose(), () => trackedFactory.CreateClient());
    }

    /// <summary>
    /// Tracks all EF Core SQL queries executed during the test.
    /// Targets all registered DbContext types.
    /// </summary>
    public static QueryGuard TrackQueries<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class
    {
        var interceptor = new QueryInterceptor();

        var trackedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType.IsGenericType &&
                                d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) &&
                                d.ImplementationFactory is not null)
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                    services.Add(new ServiceDescriptor(descriptor.ServiceType, sp =>
                    {
                        var original = (DbContextOptions)descriptor.ImplementationFactory!(sp);
                        return new DbContextOptionsBuilder(original)
                            .AddInterceptors(interceptor)
                            .Options;
                    }, descriptor.Lifetime));
                }
            });
        });

        // warm up: trigger app startup + seeding before the guard is returned
        trackedFactory.CreateClient().Dispose();
        interceptor.Reset();

        return new QueryGuard(interceptor, () => trackedFactory.Dispose(), () => trackedFactory.CreateClient());
    }
}
