using InvestLens.Shared.MessageBus.Data;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Validators;

public static class MoexValidator
{
    public static void Validate(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["MoexBaseUrl"], "MoexBaseUrl");
    }
}