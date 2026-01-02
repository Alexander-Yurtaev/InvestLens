var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

Console.WriteLine("===================================================================");

Console.WriteLine($"DB_HOST: {builder.Configuration["DB_HOST"] ?? " is NULL"}");
Console.WriteLine($"DB_NAME: {builder.Configuration["DB_NAME"] ?? " is NULL"}");
Console.WriteLine($"DB_USER: {builder.Configuration["DB_USER"] ?? " is NULL"}");
Console.WriteLine($"DB_PASSWORD: {builder.Configuration["DB_PASSWORD"] ?? " is NULL"}");

Console.WriteLine($"REDIS_HOST: {builder.Configuration["REDIS_HOST"] ?? " is NULL"}");
Console.WriteLine($"REDIS_USER: {builder.Configuration["REDIS_USER"] ?? " is NULL"}");
Console.WriteLine($"REDIS_PASSWORD: {builder.Configuration["REDIS_PASSWORD"] ?? " is NULL"}");
Console.WriteLine($"REDIS_TIMEOUT: {builder.Configuration["REDIS_TIMEOUT"] ?? " is NULL"}");
Console.WriteLine($"REDIS_SSL: {builder.Configuration["REDIS_SSL"] ?? " is NULL"}");
Console.WriteLine($"REDIS_ALLOW_ADMIN: {builder.Configuration["REDIS_ALLOW_ADMIN"] ?? " is NULL"}");

Console.WriteLine($"RABBITMQ_HOST: {builder.Configuration["RABBITMQ_HOST"] ?? " is NULL"}");
Console.WriteLine($"RABBITMQ_USER: {builder.Configuration["RABBITMQ_USER"] ?? " is NULL"}");
Console.WriteLine($"RABBITMQ_PASSWORD: {builder.Configuration["RABBITMQ_PASSWORD"] ?? " is NULL"}");
Console.WriteLine($"RABBITMQ_VHOST: {builder.Configuration["RABBITMQ_VHOST"] ?? " is NULL"}");

Console.WriteLine($"POSTGRES_PASSWORD: {builder.Configuration["POSTGRES_PASSWORD"] ?? " is NULL"}");

Console.WriteLine("===================================================================");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
