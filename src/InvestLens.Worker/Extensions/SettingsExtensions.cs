using InvestLens.Worker.Models.Settings;

namespace InvestLens.Worker.Extensions;

public static class SettingsExtensions
{
    public static IJobSettings AddJobSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var jobSettings = configuration.GetSection("JobSettings").Get<JobSettings>() ??
                             throw new ArgumentNullException("JobSettings");

        ValidateJobSettings(jobSettings);

        // Регистрация JobSettings
        services.AddSingleton<IJobSettings>(_ => jobSettings);

        return jobSettings;
    }

    #region Private Mathods

    private static void ValidateJobSettings(IJobSettings jobSettings)
    {
        if (jobSettings.DelayBetweenRefresh.TotalMinutes <= 0)
        {
            throw new ArgumentException("DelayBetweenRefresh must be grate then 0 minutes.");
        }

        if (jobSettings.MaxRetryCount <= 0)
        {
            throw new ArgumentException("MaxRetryCount must be grate then 0.");
        }
    }

    #endregion Private Mathods
}