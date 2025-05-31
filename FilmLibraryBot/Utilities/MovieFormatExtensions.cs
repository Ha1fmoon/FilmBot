using Domain.Models;

namespace FilmLibraryBot.Utilities;

public static class MovieFormatExtensions
{
    public static string FormatForSearchList(this Movie movie)
    {
        return $"{movie.Title} ({movie.Year}) {movie.Rating}/10";
    }

    public static string FormatForWatchList(this Movie movie)
    {
        return $"{movie.Title} ({movie.Year}) - " + (movie.MediaType == MediaType.Movie ? "Фильм" : "Сериал");
    }

    public static string FormatForWatchedList(this Movie movie)
    {
        return $"{movie.Title} ({movie.Year}){movie.UserRating}/10";
    }
}