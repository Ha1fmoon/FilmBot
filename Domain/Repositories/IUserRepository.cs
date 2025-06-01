using Domain.Models;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<User> TrySaveUserAsync(long userId, string username, CancellationToken cancellationToken = default);
    Task<User?> TryGetUserAsync(long userId, CancellationToken cancellationToken = default);

    Task AddToWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task RemoveFromWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task<LibraryResults> GetWatchListAsync(long userId, int page = 1, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetFullWatchListAsync(long userId, CancellationToken cancellationToken = default);

    Task AddToWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default);
    Task RemoveFromWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default);

    Task<LibraryResults> GetWatchedMoviesAsync(long userId, int page = 1,
        CancellationToken cancellationToken = default);

    Task SetRatingAsync(long userId, long movieId, int rating, CancellationToken cancellationToken = default);
    Task<int?> GetRatingAsync(long userId, long movieId, CancellationToken cancellationToken = default);

    Task<bool> IsInWatchlistAsync(long userId, long movieId, CancellationToken cancellationToken);
    Task<bool> IsInWatchedAsync(long userId, long movieId, CancellationToken cancellationToken);
}