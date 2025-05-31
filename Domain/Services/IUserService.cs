using Domain.Models;

namespace Domain.Services;

public interface IUserService
{
    Task<User?> GetOrCreateUserAsync(long userId, string username, CancellationToken cancellationToken = default);

    Task AddToWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task RemoveFromWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetWatchListAsync(long userId, CancellationToken cancellationToken = default);

    Task MarkAsWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task RemoveFromWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetWatchedMoviesAsync(long userId, CancellationToken cancellationToken = default);

    Task<Movie?> GetRandomFromWatchListAsync(long userId, MediaType? mediaType = null,
        CancellationToken cancellationToken = default);

    Task RateMovieAsync(long userId, long movieId, int rating, CancellationToken cancellationToken = default);
    Task<int?> GetRatingAsync(long userId, long movieId, CancellationToken cancellationToken = default);

    Task<bool> IsInWatchlistAsync(long userId, long movieId, CancellationToken cancellationToken);
    Task<bool> IsInWatchedAsync(long userId, long movieId, CancellationToken cancellationToken);
}