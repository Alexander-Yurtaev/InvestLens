namespace InvestLens.Abstraction.Services;

public interface IBotCommandService
{
    Task HandleCommandAsync(string command, CancellationToken cancellationToken);
}