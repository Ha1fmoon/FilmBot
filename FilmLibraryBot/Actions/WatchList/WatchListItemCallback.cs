using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.WatchList;

public class WatchListItemCallback : CallbackBase
{
    public override string Name => "watchlist_item";
    public override bool IsStateRequired => true;
    public override bool IsMovieServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var value = GetValue();
        var cancellationToken = GetCancellationToken();

        if (!long.TryParse(value, out var movieId))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.ItemSelectionError"));
            return;
        }

        StateManager.SetState(userId, "SELECTED_LIBRARY_ITEM", movieId);

        var movie = await MovieService.GetMovieDetailsAsync(movieId, cancellationToken);

        if (movie == null)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.MovieNotFoundInLibrary"));
            return;
        }

        var response = MessageFormatters.FormatMovieItem(movie, true);

        var posterUrl = movie.PosterPath;

        await EditMessageWithPhotoAsync(
            response,
            posterUrl,
            KeyboardMarkups.LibraryItemActions(movie.Id, LibraryType.WatchList));
    }
}