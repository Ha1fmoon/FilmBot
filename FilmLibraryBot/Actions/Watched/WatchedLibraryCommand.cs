using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class WatchedLibrary : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Watched");
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        var library = (await UserService.GetWatchedMoviesAsync(userId, cancellationToken)).ToList();

        if (library.Count == 0)
        {
            await SendMessageAsync(
                Texts.Localization.Get("Messages.EmptyWatchedLibrary"));
            return;
        }

        var response = Texts.Localization.Get("Messages.WatchedLibraryTitle");

        await SendMessageAsync(response, KeyboardMarkups.Library(library, LibraryType.Watched));
    }
}