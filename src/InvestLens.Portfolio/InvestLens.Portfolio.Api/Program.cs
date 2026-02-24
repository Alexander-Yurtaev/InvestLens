using CorrelationId;
using CorrelationId.DependencyInjection;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Services;
using Serilog;
using Serilog.Context;

namespace InvestLens.Portfolio.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDefaultCorrelationId(options =>
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

            // Add services to the container.

            builder.Services.AddControllers();

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

            app.MapGet("/", () => "Portfolio");

            app.Run();
        }
    }
}
