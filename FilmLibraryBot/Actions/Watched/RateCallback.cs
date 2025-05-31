using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class RateCallback : CallbackBase
{
    public override string Name => "rate";
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var valueArray = GetValue().Split(':');
        var cancellationToken = GetCancellationToken();

        if (valueArray.Length < 2)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.RatingError"));
            return;
        }

        if (!long.TryParse(valueArray[0], out var movieId))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidMovieId"));
            return;
        }

        if (!int.TryParse(valueArray[1], out var rating) || rating < 1 || rating > 10)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidRating"));
            return;
        }

        var movie = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);
        if (movie == null)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
            return;
        }

        await UserService.RateMovieAsync(userId, movieId, rating, cancellationToken);

        var mediaType = movie.MediaType == MediaType.Movie
            ? Texts.Localization.Get("MediaTypes.Movie")
            : Texts.Localization.Get("MediaTypes.TvShow");
        await EditMessageAsync(
            Texts.Localization.Get("Messages.MovieRated", mediaType, movie.Title, movie.Year.ToString(),
                rating.ToString()));
    }
}