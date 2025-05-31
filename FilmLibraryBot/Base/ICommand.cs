using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Base;

public interface ICommand
{
    public string Name { get; }
    public string Description { get; }
    public bool IsCommandListRequired { get; }
    public bool IsStateRequired { get; }
    public bool IsMovieServiceRequired { get; }
    public bool IsUserServiceRequired { get; }

    void SetCommandsList(string formattedCommandsList);
    void SetStateManager(UserStateManager stateManager);
    void SetMovieService(IMovieService movieService);
    void SetUserService(IUserService userService);
    void SetErrorLogger(IErrorLogger errorLogger);

    public Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
    public Task HandleAsync();
}