using Domain.Models;

namespace Domain.Services;

public interface IMovieService
{
    Task<SearchResults> SearchMoviesAsync(string query, MediaType mediaType, int page,
        CancellationToken cancellationToken);

    Task AddMovieToLibraryAsync(Movie movie, CancellationToken cancellationToken);
    Task<Movie?> GetMovieDetailsAsync(long movieId, CancellationToken cancellationToken);
}