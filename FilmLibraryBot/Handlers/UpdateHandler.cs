using Domain.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly CallbackQueryHandler _callbackQueryHandler;
    private readonly CommandHandler _commandHandler;
    private readonly IErrorLogger _errorLogger;
    private readonly TextHandler _textHandler;

    public UpdateHandler(CommandHandler commandHandler, CallbackQueryHandler callbackQueryHandler,
        TextHandler textHandler, IErrorLogger errorLogger)
    {
        _commandHandler = commandHandler;
        _callbackQueryHandler = callbackQueryHandler;
        _textHandler = textHandler;
        _errorLogger = errorLogger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message?.Text != null && update.Message.From != null && update.Message.Chat?.Id != null)
            {
                var message = update.Message;
                var text = message.Text;

                if (text.StartsWith('/'))
                    await _commandHandler.HandleCommandAsync(botClient, message, cancellationToken);
                else
                    await _textHandler.HandleTextMessageAsync(botClient, message, cancellationToken);
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Message?.Chat?.Id != null &&
                     update.CallbackQuery.Message.MessageId > 0 && update.CallbackQuery.From?.Id != null)
            {
                await _callbackQueryHandler.HandleCallbackQueryAsync(botClient, update.CallbackQuery,
                    cancellationToken);
            }
        }
        catch (Exception exception)
        {
            await _errorLogger.LogErrorAsync(exception, "UpdateHandler.HandleUpdateAsync");
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        await _errorLogger.LogErrorAsync(exception, $"TelegramBot.{source}");
    }
}