using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class WatchedItemCallback : CallbackBase
{
    public override string Name => "watched_item";
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var value = GetValue();
        var cancellationToken = GetCancellationToken();

        if (!long.TryParse(value, out var movieId))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidParameter", "ID"));
            return;
        }

        var movie = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);
        if (movie == null)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
            return;
        }

        movie.UserRating =
            await UserService.GetRatingAsync(userId, movie.Id, cancellationToken);

        var response = MessageFormatters.FormatMovieItem(movie, isWatched: true);

        var posterUrl = movie.PosterPath;

        await EditMessageWithPhotoAsync(
            response,
            posterUrl,
            KeyboardMarkups.LibraryItemActions(movie.Id, LibraryType.Watched));
    }
}