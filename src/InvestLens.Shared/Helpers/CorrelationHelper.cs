using Serilog.Context;

namespace InvestLens.Shared.Helpers;

public static class CorrelationHelper
{
    public static void CallLogWithCorrelationId(string correlationId, Action action)
    {
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            action();
        }
    }

    public static async Task CallLogWithCorrelationIdAsync(string correlationId, Func<Task> action)
    {
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await action();
        }
    }
}
