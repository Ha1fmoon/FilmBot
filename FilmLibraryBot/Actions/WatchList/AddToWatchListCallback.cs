using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.WatchList;

public class AddToWatchListCallback : CallbackBase
{
    public override string Name => "add_to_watchlist";
    public override bool IsStateRequired => true;
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var value = GetValue();
        var token = GetCancellationToken();

        if (!int.TryParse(value, out var index))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.ItemSelectionError"));
            return;
        }

        if (StateManager.TryGetState(userId, out var state, out var stateData) &&
            state == "MOVIE_SEARCH_RESULTS" &&
            stateData is SearchResults searchResults)
        {
            if (index < 0 || index >= searchResults.Items.Count)
            {
                await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidSelection"));
                return;
            }

            var selectedMovie = searchResults.Items[index];

            await MovieService.AddMovieToLibraryAsync(selectedMovie, token);
            await UserService.AddToWatchListAsync(userId, selectedMovie.Id, token);

            var mediaType = selectedMovie.MediaType == MediaType.Movie
                ? Texts.Localization.Get("MediaTypes.Movie")
                : Texts.Localization.Get("MediaTypes.TvShow");

            var successMessage = Texts.Localization.Get("Messages.MovieAddedToWatchList",
                mediaType, selectedMovie.Title, selectedMovie.Year.ToString());

            await DeleteMessageAndSendTextAsync(successMessage);
        }
        else
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.Error"));
        }
    }
}