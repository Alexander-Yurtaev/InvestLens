using InvestLens.Data.Api.Models.Settings;

namespace InvestLens.Data.Api.Extensions;

public static class SettingsExtensions
{
    public static ICommonSettings AddCommonSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var commonSettings = configuration.GetSection("CommonSettings").Get<CommonSettings>() ??
                             throw new InvalidOperationException("CommonSettings");

        ValidateCommonSettings(commonSettings);

        // Регистрация ICommonSettings
        services.AddSingleton<ICommonSettings>(_ => commonSettings);

        return commonSettings;
    }

    #region Private Mathods

    private static void ValidateCommonSettings(ICommonSettings commonSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(commonSettings.MoexBaseUrl, nameof(commonSettings.MoexBaseUrl));
        ArgumentException.ThrowIfNullOrEmpty(commonSettings.TargetMigration, nameof(commonSettings.TargetMigration));
        if (commonSettings.ExpiredRefreshStatus.TotalMinutes <= 0)
        {
            throw new ArgumentException("ExpiredRefreshStatus must be greater than 0 minutes.");
        }
    }

    #endregion Private Mathods
}