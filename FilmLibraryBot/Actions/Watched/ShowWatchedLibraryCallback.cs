using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class ShowWatchedLibraryCallback : CallbackBase
{
    public override string Name => "show_watched";
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        var library = (await UserService.GetWatchedMoviesAsync(userId, cancellationToken)).ToList();

        if (library.Count == 0)
        {
            await DeleteMessageAndSendTextAsync(
                Texts.Localization.Get("Messages.EmptyWatchedLibrary"));
            return;
        }

        var response = Texts.Localization.Get("Messages.WatchedLibraryTitle");

        await DeleteMessageAndSendTextAsync(response, KeyboardMarkups.Library(library, LibraryType.Watched));
    }
}