namespace KiwiQuery;

public sealed class QueryGuard : IAsyncDisposable, IDisposable
{
    private readonly QueryInterceptor _interceptor;
    private readonly Action _onDispose;
    private readonly Func<HttpClient> _clientFactory;

    internal QueryGuard(QueryInterceptor interceptor, Action onDispose, Func<HttpClient> clientFactory)
    {
        _interceptor = interceptor;
        _onDispose = onDispose;
        _clientFactory = clientFactory;
    }

    public HttpClient CreateClient()
    {
        _interceptor.Reset();
        return _clientFactory();
    }

    public int Count => _interceptor.Count;

    public void AssertCount(int exact)
    {
        if (_interceptor.Count != exact)
            throw new QueryAssertionException(
                $"Expected exactly {exact} quer{(exact == 1 ? "y" : "ies")}, but {_interceptor.Count} were executed.");
    }

    public void AssertCount(int atLeast = 0, int atMost = int.MaxValue)
    {
        if (_interceptor.Count < atLeast || _interceptor.Count > atMost)
            throw new QueryAssertionException(
                $"Expected between {atLeast} and {atMost} queries, but {_interceptor.Count} were executed.");
    }

    public void AssertNoQueries()
    {
        if (_interceptor.Count != 0)
            throw new QueryAssertionException(
                $"Expected no queries, but {_interceptor.Count} were executed.");
    }

    public void AssertAtMost(int count)
    {
        if (_interceptor.Count > count)
            throw new QueryAssertionException(
                $"Expected at most {count} quer{(count == 1 ? "y" : "ies")}, but {_interceptor.Count} were executed.");
    }

    public void AssertAtLeast(int count)
    {
        if (_interceptor.Count < count)
            throw new QueryAssertionException(
                $"Expected at least {count} quer{(count == 1 ? "y" : "ies")}, but {_interceptor.Count} were executed.");
    }

    public void Dispose() => _onDispose();

    public ValueTask DisposeAsync()
    {
        _onDispose();
        return ValueTask.CompletedTask;
    }
}
