using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Search;

public class PageCallback : CallbackBase
{
    public override string Name => "page";
    public override bool IsStateRequired => true;
    public override bool IsMovieServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var value = GetValue();
        var cancellationToken = GetCancellationToken();

        if (!StateManager.TryGetState(userId, out var state, out var data))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "STATE"));
            return;
        }

        if (state != "MOVIE_SEARCH_RESULTS")
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "STATE"));
            return;
        }

        if (data is not SearchResults newData)
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "SEARCH RESULT"));
            return;
        }

        if (!int.TryParse(value, out var page))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "PAGE"));
            return;
        }

        var searchResults = await MovieService.SearchMoviesAsync(
            newData.Query,
            newData.MediaType,
            page,
            cancellationToken
        );

        if (searchResults.Items.Count > 0)
        {
            StateManager.SetState(userId, "MOVIE_SEARCH_RESULTS", searchResults);

            var resultMessage = MessageFormatters.FormatSearchResults(searchResults);

            await DeleteMessageAndSendTextAsync(
                resultMessage,
                KeyboardMarkups.MovieSelectionWithPagination(searchResults)
            );
        }
        else
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.SearchPageIsEmpty"));
        }
    }
}