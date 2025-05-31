using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class MarkWatchedCallback : CallbackBase
{
    public override string Name => "mark_watched";
    public override bool IsStateRequired => true;
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var value = GetValue();
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        if (!long.TryParse(value, out var movieId))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidParameter", "ID"));
            return;
        }

        var movie = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);

        if (movie != null)
        {
            await UserService.MarkAsWatchedAsync(userId, movieId, cancellationToken);

            StateManager.SetState(userId, "RATING_MOVIE", movieId);

            var mediaType = movie.MediaType == MediaType.Movie
                ? Texts.Localization.Get("MediaTypes.Movie")
                : Texts.Localization.Get("MediaTypes.TvShow");
            await DeleteMessageAndSendTextAsync(
                Texts.Localization.Get("Messages.MovieMarkedAsWatched", mediaType, movie.Title, movie.Year.ToString()),
                KeyboardMarkups.Rating(movie.Id));
        }
        else
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
        }
    }
}