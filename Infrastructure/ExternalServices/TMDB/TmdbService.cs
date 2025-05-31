using Domain.Exceptions;
using Domain.Models;
using Domain.Services;
using TMDbLib.Client;

namespace Infrastructure.ExternalServices.TMDB;

public class TmdbMediaService : IExternalMediaService
{
    private const string TmdbWebUrl = "https://www.themoviedb.org";
    private const string TmdbImageUrl = "https://image.tmdb.org/t/p/w500";
    private readonly TMDbClient _client;
    private readonly IErrorLogger _logger;

    public TmdbMediaService(string apiKey, IErrorLogger logger, string language = "en-EN")
    {
        _client = new TMDbClient(apiKey);
        _client.DefaultLanguage = language;
        _logger = logger;
    }

    public async Task<SearchResults> SearchMediaAsync(string query, MediaType mediaType, int page,
        CancellationToken cancellationToken)
    {
        var searchResults = new SearchResults
        {
            Query = query,
            MediaType = mediaType,
            CurrentPage = page
        };

        try
        {
            switch (mediaType)
            {
                case MediaType.Movie:
                {
                    var movieResults = await _client.SearchMovieAsync(
                        query,
                        page,
                        cancellationToken: cancellationToken);

                    if (movieResults != null)
                    {
                        searchResults.TotalPages = movieResults.TotalPages;
                        searchResults.TotalResults = movieResults.TotalResults;

                        if (movieResults.Results != null)
                            foreach (var result in movieResults.Results)
                                searchResults.Items.Add(new Movie
                                {
                                    Id = result.Id,
                                    Title = result.Title ?? string.Empty,
                                    Year = result.ReleaseDate?.Year ?? 0,
                                    Rating = result.VoteAverage,
                                    Overview = result.Overview ?? string.Empty,
                                    PosterPath = !string.IsNullOrEmpty(result.PosterPath)
                                        ? $"{TmdbImageUrl}{result.PosterPath}"
                                        : string.Empty,
                                    MediaType = MediaType.Movie,
                                    PagePath = $"{TmdbWebUrl}/movie/{result.Id}"
                                });
                    }

                    break;
                }
                case MediaType.Tv:
                {
                    var tvResults = await _client.SearchTvShowAsync(
                        query,
                        page,
                        cancellationToken: cancellationToken);

                    if (tvResults != null)
                    {
                        searchResults.TotalPages = tvResults.TotalPages;
                        searchResults.TotalResults = tvResults.TotalResults;

                        if (tvResults.Results != null)
                            foreach (var result in tvResults.Results)
                                searchResults.Items.Add(new Movie
                                {
                                    Id = result.Id,
                                    Title = result.Name ?? string.Empty,
                                    Year = result.FirstAirDate?.Year ?? 0,
                                    Rating = result.VoteAverage,
                                    Overview = result.Overview ?? string.Empty,
                                    PosterPath = !string.IsNullOrEmpty(result.PosterPath)
                                        ? $"{TmdbImageUrl}{result.PosterPath}"
                                        : string.Empty,
                                    MediaType = MediaType.Tv,
                                    PagePath = $"{TmdbWebUrl}/tv/{result.Id}"
                                });
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"TmdbMediaService error query={query}, mediaType={mediaType}, page={page}");
        }

        return searchResults;
    }
}