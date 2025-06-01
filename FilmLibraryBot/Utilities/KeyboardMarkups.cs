using Domain.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace FilmLibraryBot.Utilities;

public static class KeyboardMarkups
{
    public static InlineKeyboardMarkup MovieSelectionWithPagination(SearchResults searchResults)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < searchResults.Items.Count; i++)
        {
            var movie = searchResults.Items[i];
            buttons.Add([
                InlineKeyboardButton.WithCallbackData(
                    movie.FormatForSearchList(),
                    $"movie_details:{i}")
            ]);
        }

        if (searchResults.TotalPages > 1)
        {
            var navigationRow = new List<InlineKeyboardButton>();

            if (searchResults.CurrentPage > 1)
                navigationRow.Add(InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.Previous"),
                    $"page:{searchResults.CurrentPage - 1}"));

            if (searchResults.CurrentPage < searchResults.TotalPages)
                navigationRow.Add(InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.Next"),
                    $"page:{searchResults.CurrentPage + 1}"));

            if (navigationRow.Any()) buttons.Add(navigationRow.ToArray());
        }

        buttons.Add([InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.Cancel"), "back")]);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup LibraryWithPagination(LibraryResults libraryResults)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < libraryResults.Items.Count; i++)
        {
            var movie = libraryResults.Items[i];
            var callbackData = libraryResults.LibraryType == LibraryType.WatchList
                ? $"watchlist_item:{movie.Id}"
                : $"watched_item:{movie.Id}";

            var formattedMovie = libraryResults.LibraryType == LibraryType.WatchList
                ? movie.FormatForWatchList()
                : movie.FormatForWatchedList();

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(formattedMovie, callbackData)
            ]);
        }

        if (libraryResults.TotalPages > 1)
        {
            var navigationRow = new List<InlineKeyboardButton>();

            if (libraryResults.CurrentPage > 1)
                navigationRow.Add(InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.Previous"),
                    $"library_page:{libraryResults.CurrentPage - 1}"));

            if (libraryResults.CurrentPage < libraryResults.TotalPages)
                navigationRow.Add(InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.Next"),
                    $"library_page:{libraryResults.CurrentPage + 1}"));

            buttons.Add(navigationRow.ToArray());
        }

        buttons.Add([InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.Cancel"), "cancel")]);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup RandomSelection()
    {
        return new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("MediaTypes.Movie"), "random:movie"),
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("MediaTypes.TvShow"), "random:tv")
            ],
            [InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("MediaTypes.Any"), "random:any")],
            [InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.Back"), "back")]
        ]);
    }

    public static InlineKeyboardMarkup LibraryItemActions(long movieId, LibraryType libraryType)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        if (libraryType == LibraryType.WatchList)
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.MarkAsWatched"),
                    $"mark_watched:{movieId}")
            ]);
            buttons.Add([
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.RemoveFromWatchListFull"),
                    $"remove_from_watchlist:{movieId}")
            ]);
        }
        else
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.RemoveFromWatchedFull"),
                    $"unmark_watched:{movieId}")
            ]);
        }

        var backCallback = libraryType == LibraryType.WatchList ? "show_watchlist" : "show_watched";
        buttons.Add([
            InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.BackToLibrary"), backCallback)
        ]);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup Rating(long movieId)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        var row1 = new List<InlineKeyboardButton>();
        var row2 = new List<InlineKeyboardButton>();

        for (var i = 1; i <= 10; i++)
        {
            var button = InlineKeyboardButton.WithCallbackData(i.ToString(), $"rate:{movieId}:{i}");
            if (i <= 5)
                row1.Add(button);
            else
                row2.Add(button);
        }

        buttons.Add(row1.ToArray());
        buttons.Add(row2.ToArray());
        buttons.Add([InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.DoNotRate"), "back")]);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup MediaTypeSelection()
    {
        return new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("MediaTypes.Movie"),
                    $"search_media_type:{MediaType.Movie}"),
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("MediaTypes.TvShow"),
                    $"search_media_type:{MediaType.Tv}")
            ]
        ]);
    }

    public static InlineKeyboardMarkup SearchResultKeyboard(int movieIndex, bool isInWatchlist, bool isWatched,
        SearchResults searchResults)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        if (!isInWatchlist && !isWatched)
            buttons.Add([
                InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.AddToWatchListFull"),
                    $"add_to_watchlist:{movieIndex}")
            ]);

        buttons.Add([
            InlineKeyboardButton.WithCallbackData(Texts.Localization.Get("Buttons.BackToSearch"),
                $"page:{searchResults.CurrentPage}")
        ]);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup LibraryTypeSelection()
    {
        return new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.WatchList"),
                    "show_watchlist"
                ),
                InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.WatchedLibrary"),
                    "show_watched"
                ),
                InlineKeyboardButton.WithCallbackData(
                    Texts.Localization.Get("Buttons.Cancel"),
                    "cancel"
                )
            ]
        ]);
    }
}