using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KiwiQuery;

internal sealed class QueryInterceptor : DbCommandInterceptor
{
    private int _count;

    public int Count => _count;

    public void Reset() => Interlocked.Exchange(ref _count, 0);

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (IsUserQuery(command.CommandText))
            Interlocked.Increment(ref _count);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        if (IsUserQuery(command.CommandText))
            Interlocked.Increment(ref _count);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private static bool IsUserQuery(string sql)
    {
        var trimmed = sql.TrimStart();
        return trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
    }
}
