using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.WatchList;

public class ShowWatchListCallback : CallbackBase
{
    public override string Name => "show_watchlist";
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();

        var library = (await UserService.GetWatchListAsync(userId, cancellationToken)).ToList();

        if (library.Count == 0)
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.EmptyWatchList"));
            return;
        }

        var response = Texts.Localization.Get("Messages.WatchListTitle");

        await DeleteMessageAndSendTextAsync(response, KeyboardMarkups.Library(library, LibraryType.WatchList));
    }
}