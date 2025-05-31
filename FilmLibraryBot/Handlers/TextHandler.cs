using Domain.Models;
using Domain.Services;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Handlers;

public class TextHandler
{
    private readonly IMovieService _movieService;
    private readonly UserStateManager _userStateManager;

    public TextHandler(UserStateManager userStateManager, IMovieService movieService)
    {
        _userStateManager = userStateManager;
        _movieService = movieService;
    }

    public async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var userId = message.From!.Id;
        var chatId = message.Chat.Id;
        var text = message.Text ?? string.Empty;

        if (!_userStateManager.TryGetState(userId, out var state, out _))
        {
            await botClient.SendMessage(
                chatId,
                Texts.Localization.Get("Messages.UnknownCommand"),
                cancellationToken: cancellationToken
            );
            return;
        }

        if (string.IsNullOrEmpty(message.Text))
        {
            await botClient.SendMessage(
                chatId,
                Texts.Localization.Get("Messages.EnterSearchQuery"),
                cancellationToken: cancellationToken
            );
            return;
        }

        if (state == "WAITING_FOR_MOVIE_NAME") await HandleMovieNameInput(botClient, message, text, cancellationToken);
    }

    private async Task HandleMovieNameInput(ITelegramBotClient botClient, Message message, string movieName,
        CancellationToken cancellationToken)
    {
        var userId = message.From!.Id;
        var chatId = message.Chat.Id;

        _userStateManager.TryGetState(userId, out _, out var mediaTypeObj);

        if (mediaTypeObj is not MediaType mediaType)
        {
            await botClient.SendMessage(
                chatId,
                Texts.Localization.Get("Messages.ContentTypeNotSpecified"),
                cancellationToken: cancellationToken
            );

            _userStateManager.ClearState(userId);
            return;
        }

        var searchResults = await _movieService.SearchMoviesAsync(movieName, mediaType, 1, cancellationToken);

        if (searchResults.Items.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                Texts.Localization.Get("Messages.SearchResultsIsEmpty"),
                cancellationToken: cancellationToken
            );
            return;
        }

        _userStateManager.SetState(userId, "MOVIE_SEARCH_RESULTS", searchResults);

        var resultMessage = MessageFormatters.FormatSearchResults(searchResults);

        await botClient.SendMessage(
            chatId,
            resultMessage,
            replyMarkup: KeyboardMarkups.MovieSelectionWithPagination(searchResults),
            cancellationToken: cancellationToken
        );
    }
}