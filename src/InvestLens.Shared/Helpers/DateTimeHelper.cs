using InvestLens.Data.Entities;

namespace InvestLens.Shared.Helpers;

public static class DateTimeHelper
{
    public static bool IsExpired(DateTime start, int expired)
    {
        return start.AddHours(expired) < DateTime.UtcNow;
    }
}
