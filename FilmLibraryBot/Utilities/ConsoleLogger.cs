using Domain.Exceptions;

namespace FilmLibraryBot.Utilities;

public class ConsoleLogger : IErrorLogger
{
    public Task LogErrorAsync(Exception ex, string context)
    {
        Console.Error.WriteLine($"[ERROR] [{DateTime.Now}] [{context}]: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);

        return Task.CompletedTask;
    }

    public Task LogWarningAsync(string message, string context)
    {
        Console.Error.WriteLine($"[WARNING] [{DateTime.Now}] [{context}]: {message}");
        return Task.CompletedTask;
    }
}