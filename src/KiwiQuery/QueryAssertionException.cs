namespace KiwiQuery;

public sealed class QueryAssertionException : Exception
{
    public QueryAssertionException(string message) : base(message) { }
}
