using Hangfire.Dashboard;

namespace InvestLens.Shared.Filters;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;

        //var httpContext = context.GetHttpContext();

        //// Проверка по заголовку (например, для внутренних сервисов)
        //var hasValidApiKey = httpContext.Request.Headers["X-API-Key"] == "your-secret-key";

        //// Или проверка аутентификации
        //return httpContext.User.Identity?.IsAuthenticated == true
        //       && httpContext.User.IsInRole("Admin");
    }
}