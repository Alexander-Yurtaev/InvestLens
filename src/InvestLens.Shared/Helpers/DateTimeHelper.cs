namespace InvestLens.Shared.Helpers;

public static class DateTimeHelper
{
    public static bool IsRefreshed(DateTime start, TimeSpan expired)
    {
        return start.AddSeconds(expired.TotalSeconds) > DateTime.UtcNow;
    }
}
