using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Common;

public class LibraryPageCallback : CallbackBase
{
    public override string Name => "library_page";
    public override bool IsStateRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var value = GetValue();
        var cancellationToken = GetCancellationToken();

        if (!StateManager.TryGetState(userId, out var state, out var data))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "STATE"));
            return;
        }

        if (state != "LIBRARY_RESULTS")
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "STATE"));
            return;
        }

        if (data is not LibraryResults currentLibraryData)
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "LIBRARY RESULT"));
            return;
        }

        if (!int.TryParse(value, out var page))
        {
            await DeleteMessageAndSendTextAsync(Texts.Localization.Get("Messages.InvalidParameter", "PAGE"));
            return;
        }

        LibraryResults libraryResults;
        switch (currentLibraryData.LibraryType)
        {
            case LibraryType.WatchList:
                libraryResults = await UserService.GetWatchListAsync(userId, page, cancellationToken);
                break;
            case LibraryType.Watched:
                libraryResults = await UserService.GetWatchedMoviesAsync(userId, page, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported library type: {currentLibraryData.LibraryType}");
        }

        if (libraryResults.Items.Count == 0)
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.SearchPageIsEmpty"));
            return;
        }

        StateManager.SetState(userId, "LIBRARY_RESULTS", libraryResults);

        var response = MessageFormatters.FormatLibraryResults(libraryResults);
        await EditMessageAsync(response, KeyboardMarkups.LibraryWithPagination(libraryResults));
    }
}