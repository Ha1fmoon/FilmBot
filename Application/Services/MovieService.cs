using Domain.Exceptions;
using Domain.Models;
using Domain.Repositories;
using Domain.Services;

namespace Application.Services;

public class MovieService : IMovieService
{
    private readonly IExternalMediaService _externalMediaService;
    private readonly IErrorLogger _logger;
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository, IExternalMediaService externalMediaService,
        IErrorLogger errorLogger)
    {
        _movieRepository = movieRepository;
        _externalMediaService = externalMediaService;
        _logger = errorLogger;
    }

    public async Task<SearchResults> SearchMoviesAsync(string query, MediaType mediaType, int page,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(query))
                return await _externalMediaService.SearchMediaAsync(query, mediaType, page, cancellationToken);

            await _logger.LogWarningAsync("Attempted search with empty query", "MovieService.SearchMoviesAsync");

            return new SearchResults();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"MovieService.SearchMoviesAsync: query={query}, mediaType={mediaType}, page={page}");

            return new SearchResults();
        }
    }

    public async Task<Movie?> GetMovieDetailsAsync(long movieId, CancellationToken cancellationToken)
    {
        try
        {
            return await _movieRepository.GetByIdAsync(movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"MovieService.GetMovieDetailsAsync: movieId={movieId}");
            return null;
        }
    }

    public async Task AddMovieToLibraryAsync(Movie movie, CancellationToken cancellationToken)
    {
        try
        {
            var isMovieExists = await _movieRepository.ExistsAsync(movie.Id, cancellationToken);
            if (isMovieExists) return;

            await _movieRepository.AddAsync(movie, cancellationToken);

            var verifyExists = await _movieRepository.ExistsAsync(movie.Id, cancellationToken);
            if (!verifyExists) throw new InvalidOperationException($"Failed to add movie {movie.Id} to database");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"MovieService.AddMovieToLibraryAsync: movieId={movie.Id}, title={movie.Title}");
        }
    }
}