using CorrelationId;
using CorrelationId.DependencyInjection;
using InvestLens.Auth.Api.Models;
using InvestLens.Auth.DataContext;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;

namespace InvestLens.Auth.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCorrelationId(options =>
            {
                options.RequestHeader = HeaderConstants.CorrelationHeader;
                options.ResponseHeader = HeaderConstants.CorrelationHeader;
                options.IncludeInResponse = true;
                options.AddToLoggingScope = true;
                options.LoggingScopeKey = "CorrelationId";
            });

            builder.Services.AddSingleton<ICorrelationIdService, CorrelationIdService>();

            Log.Logger = SerilogHelper.CreateLogger(builder);
            builder.Host.UseSerilog((_, configuration) =>
            {
                configuration
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
            });

            try
            {
                // Add services to the container.
                var commonSettings = builder.Configuration
                                         .GetSection("CommonSettings")
                                         .Get<CommonSettings>()
                                     ??
                                     throw new InvalidOperationException("CommonSettings");
                builder.Services.AddSingleton(commonSettings);

                builder.Services.AddControllers();

                builder.Services.AddScoped<IPollyService, PollyService>();
                builder.Services.AddDbContext<InvestLensAuthContext>((provider, options) =>
                {
                    var connectionString =
                        ConnectionStringHelper.GetTargetConnectionString(provider.GetRequiredService<IConfiguration>());
                    options.UseNpgsql(connectionString);
                });

                var app = builder.Build();

                app.Use(async (context, next) =>
                {
                    var correlationIdService = context.RequestServices.GetRequiredService<ICorrelationIdService>();

                    // 1. Получаем или создаем CorrelationId
                    var correlationId = context.Request.Headers[HeaderConstants.CorrelationHeader].FirstOrDefault();

                    if (string.IsNullOrEmpty(correlationId))
                    {
                        correlationId = correlationIdService.GetOrCreateCorrelationId("use");
                        context.Request.Headers.Append(HeaderConstants.CorrelationHeader, correlationId);
                        Log.Information("Generated new CorrelationId.");
                    }
                    else
                    {
                        // 2. Сохраняем в CorrelationContext
                        correlationIdService.SetCorrelationId(correlationId);
                    }

                    // 3. Добавляем CorrelationId в заголовок ответа
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers.Append(HeaderConstants.CorrelationHeader, correlationId);
                        return Task.CompletedTask;
                    });

                    // 4. Добавляем в логи через LogContext
                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        await next(context);
                    }
                });

                app.UseCorrelationId();

                // 3. Использование Serilog для логирования запросов
                app.UseSerilogRequestLogging();

                // Configure the HTTP request pipeline.

                app.UseHttpsRedirection();

                app.UseAuthorization();


                app.MapControllers();

                await EnsureDatabaseInitAsync(app);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated due to an exception");
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        #region Private Methods

        private static async Task EnsureDatabaseInitAsync(WebApplication app)
        {
            try
            {
                using var scope = app.Services.CreateScope();

                // 1. Ожидаем готовности Postgres
                await DatabaseHelper.WaitForPostgresAsync(app.Configuration);

                // 2. Создаем БД
                var pollyPolicy = scope.ServiceProvider.GetRequiredService<IPollyService>();
                await DatabaseHelper.EnsureDatabaseCreatedAsync(pollyPolicy, app.Configuration, true);

                // 3. Получаем целевую миграцию
                var settings = scope.ServiceProvider.GetRequiredService<CommonSettings>();

                // 4. Применяем миграции
                if (string.IsNullOrEmpty(settings.TargetMigration))
                {
                    await DatabaseHelper.ApplyMigrationsAsync<InvestLensAuthContext>(app.Services);
                }
                else
                {
                    await DatabaseHelper.ApplyMigrationsAsync<InvestLensAuthContext>(app.Services,
                        settings.TargetMigration);
                }

                Log.Information("Database initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal database initialization error");
                throw;
            }
        }

        #endregion Private Methods
    }
}
