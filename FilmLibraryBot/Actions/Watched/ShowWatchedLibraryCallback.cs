using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Watched;

public class ShowWatchedLibraryCallback : CallbackBase
{
    public override string Name => "show_watched";
    public override bool IsUserServiceRequired => true;
    public override bool IsStateRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var cancellationToken = GetCancellationToken();
        var page = 1;

        if (!string.IsNullOrEmpty(GetValue()) && int.TryParse(GetValue(), out var requestedPage)) page = requestedPage;

        var libraryResults = await UserService.GetWatchedMoviesAsync(userId, page, cancellationToken);

        if (libraryResults.TotalResults == 0)
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.EmptyWatchedLibrary"));
            return;
        }

        if (libraryResults.Items.Count == 0 && page > 1)
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.SearchPageIsEmpty"));
            return;
        }

        StateManager.SetState(userId, "LIBRARY_RESULTS", libraryResults);

        var response = MessageFormatters.FormatLibraryResults(libraryResults);
        await DeleteMessageAndSendTextAsync(response, KeyboardMarkups.LibraryWithPagination(libraryResults));
    }
}