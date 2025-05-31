using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Base;

public interface ICallback
{
    string Name { get; }
    bool IsStateRequired { get; }
    public bool IsMovieServiceRequired { get; }
    public bool IsUserServiceRequired { get; }

    void SetStateManager(UserStateManager stateManager);
    void SetMovieService(IMovieService movieService);
    void SetUserService(IUserService userService);
    void SetErrorLogger(IErrorLogger errorLogger);

    public Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string value,
        CancellationToken cancellationToken);

    public Task HandleAsync();
}