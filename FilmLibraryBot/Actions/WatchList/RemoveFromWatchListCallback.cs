using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.WatchList;

public class RemoveFromWatchListCallback : CallbackBase
{
    public override string Name => "remove_from_watchlist";
    public override bool IsStateRequired => true;
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        if (!long.TryParse(GetValue(), out var movieId))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidParameter", "ID"));
            return;
        }

        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        var movieToDelete = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);

        if (movieToDelete != null)
        {
            await UserService.RemoveFromWatchListAsync(userId, movieId, cancellationToken);

            var mediaType = movieToDelete.MediaType == MediaType.Movie
                ? Texts.Localization.Get("MediaTypes.Movie")
                : Texts.Localization.Get("MediaTypes.TvShow");

            await DeleteMessageAndSendTextAsync(
                Texts.Localization.Get("Messages.MovieRemovedFromWatchList",
                    mediaType, movieToDelete.Title, movieToDelete.Year.ToString()));
        }
        else
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
        }

        StateManager.ClearState(userId);
    }
}