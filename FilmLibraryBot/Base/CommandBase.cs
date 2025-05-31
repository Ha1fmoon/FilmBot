using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Exceptions;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FilmLibraryBot.Base;

public abstract class CommandBase : ICommand
{
    private string? _commandsList;
    private IErrorLogger? _errorLogger;
    private IMovieService? _movieService;
    private UserStateManager? _stateManager;
    private IUserService? _userService;

    protected IMovieService MovieService
    {
        get
        {
            if (!IsMovieServiceRequired)
                throw new InvalidOperationException(ExceptionMessages.MovieServiceRequired);

            if (_movieService == null)
                throw new InvalidOperationException(ExceptionMessages.MovieServiceNotInitialized);

            return _movieService;
        }
    }

    protected IUserService UserService
    {
        get
        {
            if (!IsUserServiceRequired)
                throw new InvalidOperationException(ExceptionMessages.UserServiceRequired);

            if (_userService == null)
                throw new InvalidOperationException(ExceptionMessages.UserServiceNotInitialized);

            return _userService;
        }
    }

    protected UserStateManager StateManager
    {
        get
        {
            if (!IsStateRequired)
                throw new InvalidOperationException(ExceptionMessages.StateManagerRequired);

            if (_stateManager == null)
                throw new InvalidOperationException(ExceptionMessages.StateManagerNotInitialized);

            return _stateManager;
        }
    }

    protected string CommandsList
    {
        get
        {
            if (!IsCommandListRequired)
                throw new InvalidOperationException(ExceptionMessages.CommandListRequired);

            if (_commandsList == null)
                throw new InvalidOperationException(ExceptionMessages.CommandsListNotInitialized);

            return _commandsList;
        }
    }

    private ITelegramBotClient BotClient { get; set; } = null!;
    private Message Message { get; set; } = null!;
    protected CancellationToken CancellationToken { get; set; }

    public virtual string Name => GetType().Name.ToLower();
    public abstract string Description { get; }

    public virtual bool IsCommandListRequired => false;
    public virtual bool IsStateRequired => false;
    public virtual bool IsMovieServiceRequired => false;
    public virtual bool IsUserServiceRequired => false;

    public void SetCommandsList(string formattedCommandsList)
    {
        _commandsList = formattedCommandsList;
    }

    public void SetStateManager(UserStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void SetMovieService(IMovieService movieService)
    {
        _movieService = movieService;
    }

    public void SetUserService(IUserService userService)
    {
        _userService = userService;
    }

    public void SetErrorLogger(IErrorLogger errorLogger)
    {
        _errorLogger = errorLogger;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        BotClient = botClient;
        Message = message;
        CancellationToken = cancellationToken;

        try
        {
            await HandleAsync();
        }
        catch (Exception ex)
        {
            if (_errorLogger != null) await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.HandleAsync");

            await SendMessageAsync(Texts.Localization.Get("Messages.Error"));
        }
    }

    public abstract Task HandleAsync();

    protected long GetChatId()
    {
        return Message.Chat.Id;
    }

    protected long GetUserId()
    {
        return Message.From!.Id;
    }

    protected CancellationToken GetCancellationToken()
    {
        return CancellationToken;
    }

    protected ITelegramBotClient GetBotClient()
    {
        return BotClient;
    }

    protected Message GetMessage()
    {
        return Message;
    }

    protected async Task SendMessageAsync(string text, ReplyMarkup? replyMarkup = null)
    {
        try
        {
            await BotClient.SendMessage(
                Message.Chat.Id,
                text,
                ParseMode.Markdown,
                replyMarkup: replyMarkup,
                cancellationToken: CancellationToken
            );
        }
        catch (Exception ex)
        {
            if (_errorLogger != null) await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.SendMessageAsync");
        }
    }
}