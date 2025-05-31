namespace Domain.Exceptions;

public interface IErrorLogger
{
    Task LogErrorAsync(Exception ex, string context);
    Task LogWarningAsync(string message, string context);
}