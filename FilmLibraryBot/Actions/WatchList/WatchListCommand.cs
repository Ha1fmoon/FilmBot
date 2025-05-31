using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.WatchList;

public class WatchList : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.WatchList");
    public override bool IsMovieServiceRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        var library = (await UserService.GetWatchListAsync(userId, cancellationToken)).ToList();

        if (library.Count == 0)
        {
            await SendMessageAsync(Texts.Localization.Get("Messages.EmptyWatchList"));
            return;
        }

        var response = Texts.Localization.Get("Messages.WatchListTitle");

        await SendMessageAsync(response, KeyboardMarkups.Library(library, LibraryType.WatchList));
    }
}