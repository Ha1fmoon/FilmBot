using System.Text.RegularExpressions;
using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Exceptions;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FilmLibraryBot.Base;

public abstract class CallbackBase : ICallback
{
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

    private ITelegramBotClient BotClient { get; set; } = null!;
    private CallbackQuery CallbackQuery { get; set; } = null!;
    private string CallbackValue { get; set; } = string.Empty;
    private CancellationToken CancellationToken { get; set; }

    public virtual string Name => ToCallbackName(GetType().Name);

    public virtual bool IsStateRequired => false;
    public virtual bool IsMovieServiceRequired => false;
    public virtual bool IsUserServiceRequired => false;

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

    public async Task ExecuteAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string value,
        CancellationToken cancellationToken)
    {
        BotClient = botClient;
        CallbackQuery = callbackQuery;
        CallbackValue = value;
        CancellationToken = cancellationToken;

        try
        {
            await HandleAsync();
        }
        catch (Exception ex)
        {
            if (_errorLogger != null) await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.HandleAsync");

            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.Error"));
        }
    }

    public abstract Task HandleAsync();

    private static string ToCallbackName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var callbackName = Regex.Replace(name, @"([A-Z])", "_$1").ToLower();

        if (callbackName.StartsWith("_")) callbackName = callbackName.Substring(1);

        return callbackName;
    }

    protected long GetChatId()
    {
        return CallbackQuery.Message!.Chat.Id;
    }

    protected int GetMessageId()
    {
        return CallbackQuery.Message!.MessageId;
    }

    protected long GetUserId()
    {
        return CallbackQuery.From.Id;
    }

    protected string GetValue()
    {
        return CallbackValue;
    }

    protected ITelegramBotClient GetBotClient()
    {
        return BotClient;
    }

    protected CancellationToken GetCancellationToken()
    {
        return CancellationToken;
    }

    protected async Task EditMessageAsync(string text, InlineKeyboardMarkup? replyMarkup = null)
    {
        try
        {
            await BotClient.EditMessageText(
                GetChatId(),
                GetMessageId(),
                text,
                ParseMode.Markdown,
                replyMarkup: replyMarkup,
                cancellationToken: CancellationToken
            );
        }
        catch (Exception ex)
        {
            if (_errorLogger != null) await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.EditMessageAsync");
        }
    }

    protected async Task EditMessageWithPhotoAsync(string text, string photoUrl, InlineKeyboardMarkup replyMarkup)
    {
        if (CallbackQuery.Message == null)
            return;

        if (string.IsNullOrEmpty(photoUrl))
        {
            await EditMessageAsync(text, replyMarkup);
            return;
        }

        var callbackQuery = CallbackQuery;
        var botClient = GetBotClient();
        var cancellationToken = GetCancellationToken();

        try
        {
            if (callbackQuery.Message.Photo != null && callbackQuery.Message.Photo.Length > 0)
            {
                await botClient.EditMessageMedia(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    new InputMediaPhoto(InputFile.FromUri(photoUrl)) { Caption = text },
                    replyMarkup,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.DeleteMessage(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    cancellationToken);

                await botClient.SendPhoto(
                    callbackQuery.Message.Chat.Id,
                    InputFile.FromUri(photoUrl),
                    text,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (_errorLogger != null)
                await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.EditMessageWithPhotoAsync");

            await DeleteMessageAndSendTextAsync(text + $"\n\n{Texts.Localization.Get("Messages.ImageLoadError")}",
                replyMarkup);
        }
    }

    protected async Task DeleteMessageAndSendTextAsync(string text, InlineKeyboardMarkup? replyMarkup = null)
    {
        var callbackQuery = CallbackQuery;
        var botClient = GetBotClient();
        var cancellationToken = GetCancellationToken();

        try
        {
            try
            {
                await botClient.DeleteMessage(
                    callbackQuery.Message!.Chat.Id,
                    callbackQuery.Message.MessageId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                if (_errorLogger != null)
                    await _errorLogger.LogErrorAsync(ex,
                        $"{GetType().Name}.DeleteMessageAndSendTextAsync - DeleteMessage");
            }

            await botClient.SendMessage(
                callbackQuery.Message!.Chat.Id,
                text,
                ParseMode.Markdown,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            if (_errorLogger != null)
                await _errorLogger.LogErrorAsync(ex, $"{GetType().Name}.DeleteMessageAndSendTextAsync - SendMessage");
        }
    }
}