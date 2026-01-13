using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace InvestLens.Shared.Helpers;

public static class RabbitMqHelper
{
    private static readonly SemaphoreSlim EnsureCheckLock = new(1, 1);

    public static async Task EnsureRabbitMqIsRunningAsync(IConfiguration configuration, CancellationToken cancellation)
    {
        await EnsureCheckLock.WaitAsync(cancellation);
        var client = new HttpClient();

        try
        {
            RabbitMqValidator.Validate(configuration);

            Log.Information("Waiting for RabbitMQ at {RabbitMqHost}...", configuration["RABBITMQ_HOST"]);

            for (int i = 0; i < 60; i++)
            {
                try
                {
                    var request = CreateHttpRequest(configuration);
                    var response = await client.SendAsync(request, cancellation);
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information("RabbitMQ is up!");
                        break;
                    }
                }
                catch (System.Net.Http.HttpRequestException ex)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed while check RabbitMQ.");
                }

                Log.Information($"Attempt {i + 1}/60: RabbitMQ not ready, waiting...");
                await Task.Delay(2000, cancellation);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{ex.Message}");
            throw;
        }
        finally
        {
            EnsureCheckLock.Release();
            client?.Dispose();
        }
    }

    #region Private Methods

    public static HttpRequestMessage CreateHttpRequest(IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RABBITMQ_HOST"];
        var username = configuration["RABBITMQ_USER"];
        var password = configuration["RABBITMQ_PASSWORD"];

        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get,
            $"http://{rabbitMqHost}:15672/api/healthchecks/node");
        var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

        return request;
    }

    #endregion Private Methods
}