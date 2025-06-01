using System.Globalization;
using Domain.Models;

namespace FilmLibraryBot.Utilities;

public static class MessageFormatters
{
    public static string FormatSearchResults(SearchResults searchResults)
    {
        var stringType = searchResults.MediaType == MediaType.Movie
            ? Texts.Localization.Get("MediaTypes.MovieGenitive")
            : Texts.Localization.Get("MediaTypes.TvShowGenitive");

        var message = Texts.Localization.Get("Messages.SearchResults",
            stringType,
            searchResults.Query,
            searchResults.CurrentPage.ToString(),
            searchResults.TotalPages.ToString()) + "\n";

        return message;
    }

    public static string FormatLibraryResults(LibraryResults libraryResults)
    {
        var libraryTypeName = libraryResults.LibraryType == LibraryType.WatchList
            ? Texts.Localization.Get("Messages.WatchListTitle")
            : Texts.Localization.Get("Messages.WatchedLibraryTitle");

        if (libraryResults.TotalPages <= 1) return libraryTypeName;

        return $"{libraryTypeName}\n" +
               Texts.Localization.Get("Messages.LibraryPagination",
                   libraryResults.CurrentPage.ToString(),
                   libraryResults.TotalPages.ToString());
    }

    public static string FormatMovieItem(Movie movie, bool isInWatchlist = false, bool isWatched = false)
    {
        var response = $"<b>{movie.Title} ({movie.Year})</b>\n";
        response += Texts.Localization.Get("Messages.Rating", movie.Rating.ToString(CultureInfo.InvariantCulture)) +
                    "\n";

        if (isWatched)
        {
            response += "\n<b>" + Texts.Localization.Get("Messages.AlreadyWatchedStatus") + "</b>\n";
            if (movie.UserRating > 0)
                response += Texts.Localization.Get("Messages.YourRating", movie.UserRating.ToString() ?? string.Empty) +
                            "\n";
            else
                response += Texts.Localization.Get("Messages.NotRatedYet") + "\n";
        }
        else if (isInWatchlist)
        {
            response += "\n<b>" + Texts.Localization.Get("Messages.AddedToLibraryStatus") + "</b>\n";
        }

        response += "\n";

        if (!string.IsNullOrEmpty(movie.Overview))
        {
            var overview = movie.Overview;
            const int maxOverviewLength = 600;

            if (overview.Length > maxOverviewLength) overview = overview.Substring(0, maxOverviewLength) + "...";

            response += $"{overview}\n\n";
        }

        if (string.IsNullOrEmpty(movie.PagePath)) return response;

        var stringType = movie.MediaType == MediaType.Movie
            ? Texts.Localization.Get("MediaTypes.MovieLocative")
            : Texts.Localization.Get("MediaTypes.TvShowLocative");

        response += $"<a href=\"{movie.PagePath}\">" + Texts.Localization.Get("Messages.MoreDetailsAbout", stringType) +
                    "</a>\n";

        const int telegramCaptionLimit = 1024;
        if (response.Length > telegramCaptionLimit) response = response.Substring(0, telegramCaptionLimit - 3) + "...";

        return response;
    }
}