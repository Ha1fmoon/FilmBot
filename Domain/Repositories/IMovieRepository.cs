using Domain.Models;

namespace Domain.Repositories;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(long movieId, CancellationToken cancellationToken = default);
    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long movieId, CancellationToken cancellationToken = default);
}