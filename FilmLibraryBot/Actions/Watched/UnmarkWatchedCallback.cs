using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class UnmarkWatchedCallback : CallbackBase
{
    public override string Name => "unmark_watched";
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;
    public override bool IsStateRequired => true;

    public override async Task HandleAsync()
    {
        var value = GetValue();
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        if (!long.TryParse(value, out var movieId))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "ID"));
            return;
        }

        var movie = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);

        if (movie != null)
        {
            await UserService.RemoveFromWatchedAsync(userId, movieId, cancellationToken);
            await UserService.AddToWatchListAsync(userId, movieId, cancellationToken);

            var mediaType = movie.MediaType == MediaType.Movie
                ? Texts.Localization.Get("MediaTypes.Movie")
                : Texts.Localization.Get("MediaTypes.TvShow");
            await DeleteMessageAndSendTextAsync(
                Texts.Localization.Get("Messages.MovieReturnedToWatchList", mediaType, movie.Title,
                    movie.Year.ToString()));
        }
        else
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
        }

        StateManager.ClearState(userId);
    }
}