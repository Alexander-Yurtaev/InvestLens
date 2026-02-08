namespace InvestLens.Shared.Interfaces.Services;

public interface IBotCommandService
{
    Task HandleCommandAsync(string command, CancellationToken cancellationToken);
}