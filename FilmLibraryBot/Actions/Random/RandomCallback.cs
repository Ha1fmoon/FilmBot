using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Random;

public class RandomCallback : CallbackBase
{
    public override string Name => "random";
    public override bool IsStateRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var mediaTypeValue = GetValue();
        var cancellationToken = GetCancellationToken();

        MediaType? mediaType;

        switch (mediaTypeValue)
        {
            case "movie":
                mediaType = MediaType.Movie;
                break;
            case "tv":
                mediaType = MediaType.Tv;
                break;
            default:
                mediaType = null;
                break;
        }

        var randomMovie = await UserService.GetRandomFromWatchListAsync(userId, mediaType, cancellationToken);

        if (randomMovie == null)
        {
            string? typeText;

            switch (mediaTypeValue)
            {
                case "movie":
                    typeText = Texts.Localization.Get("MediaTypes.MoviesGenitive");
                    break;
                case "tv":
                    typeText = Texts.Localization.Get("MediaTypes.TvShowsGenitive");
                    break;
                default:
                    typeText = Texts.Localization.Get("MediaTypes.AnyGenitive");
                    break;
            }

            await EditMessageAsync(Texts.Localization.Get("Messages.RandomMoviesNotFound", typeText));
        }
        else
        {
            var mediaTypeText = randomMovie.MediaType == MediaType.Movie
                ? Texts.Localization.Get("MediaTypes.Movie")
                : Texts.Localization.Get("MediaTypes.TvShow");

            await EditMessageAsync(Texts.Localization.Get("Messages.RandomResult", mediaTypeText) +
                                   $"{randomMovie.Title} ({randomMovie.Year}), {randomMovie.Rating}/10");
        }
    }
}