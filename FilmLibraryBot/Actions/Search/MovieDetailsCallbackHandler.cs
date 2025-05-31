using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Search;

public class MovieDetailsCallback : CallbackBase
{
    public override string Name => "movie_details";
    public override bool IsUserServiceRequired => true;
    public override bool IsStateRequired => true;

    public override async Task HandleAsync()
    {
        var value = GetValue();
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        if (!int.TryParse(value, out var index))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidParameter", "ID"));
            return;
        }

        if (!StateManager.TryGetState(userId, out var state, out var stateData) ||
            state != "MOVIE_SEARCH_RESULTS" ||
            stateData is not SearchResults searchResults)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.CallbackError"));
            return;
        }

        if (index < 0 || index >= searchResults.Items.Count)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidSelection"));
            return;
        }

        var selectedMovie = searchResults.Items[index];

        var isWatched = await UserService.IsInWatchedAsync(userId, selectedMovie.Id, cancellationToken);

        if (isWatched)
            selectedMovie.UserRating =
                await UserService.GetRatingAsync(userId, selectedMovie.Id, cancellationToken);

        var isInWatchlist = await UserService.IsInWatchlistAsync(userId, selectedMovie.Id, cancellationToken);

        var response = MessageFormatters.FormatMovieItem(selectedMovie, isInWatchlist, isWatched);

        var posterUrl = selectedMovie.PosterPath;

        var keyboard = KeyboardMarkups.SearchResultKeyboard(index, isInWatchlist, isWatched, searchResults);

        await EditMessageWithPhotoAsync(response, posterUrl, keyboard);
    }
}