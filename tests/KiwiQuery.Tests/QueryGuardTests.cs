using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace KiwiQuery.Tests;

public class QueryGuardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public QueryGuardTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TrackQueries_SingleEndpoint_CountsCorrectly()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");

        Assert.Equal(1, guard.Count);
    }

    [Fact]
    public async Task AssertCount_Exact_PassesWhenCorrect()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");

        guard.AssertCount(exact: 1);
    }

    [Fact]
    public async Task AssertCount_Exact_ThrowsWhenWrong()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");

        Assert.Throws<QueryAssertionException>(() => guard.AssertCount(exact: 5));
    }

    [Fact]
    public async Task AssertAtMost_PassesWhenUnderLimit()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");

        guard.AssertAtMost(3);
    }

    [Fact]
    public async Task AssertAtMost_ThrowsWhenOverLimit()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");
        await client.GetAsync("/products");

        Assert.Throws<QueryAssertionException>(() => guard.AssertAtMost(1));
    }

    [Fact]
    public async Task AssertNoQueries_ThrowsWhenQueriesExecuted()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");

        Assert.Throws<QueryAssertionException>(() => guard.AssertNoQueries());
    }

    [Fact]
    public async Task AssertAtLeast_PassesWhenEnoughQueries()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");
        await client.GetAsync("/products");

        guard.AssertAtLeast(2);
    }

    [Fact]
    public async Task TrackQueries_MultipleRequests_AccumulatesCount()
    {
        await using var guard = _factory.TrackQueries<Program, KiwiQuery.SampleApi.AppDbContext>();
        var client = guard.CreateClient();

        await client.GetAsync("/products");
        await client.GetAsync("/products");
        await client.GetAsync("/products");

        Assert.Equal(3, guard.Count);
    }
}
