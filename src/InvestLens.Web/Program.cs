using InvestLens.Shared.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка Serilog
Log.Logger = SerilogHelper.CreateLogger(builder);

// 2. Добавление Serilog в DI
builder.Host.UseSerilog();

try
{
    // Add services to the container.
    builder.Services.AddRazorPages();

    var app = builder.Build();

    // 3. Использование Serilog для логирования запросов
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapRazorPages()
        .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение остановлено из‑за исключения");
}
finally
{
    Log.CloseAndFlush();
}
