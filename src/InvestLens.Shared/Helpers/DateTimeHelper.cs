using InvestLens.Data.Entities;

namespace InvestLens.Shared.Helpers;

public static class DateTimeHelper
{
    public static bool IsRefreshed(DateTime start, int expired)
    {
        return start.AddMinutes(expired) > DateTime.UtcNow;
    }
}
