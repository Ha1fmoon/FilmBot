using Dapper;
using Domain.Models;
using Domain.Repositories;
using Npgsql;

namespace Infrastructure.LocalRepositories.FilmLibraryDataBase;

public class DapperUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public DapperUserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User> TrySaveUserAsync(long userId, string username,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             INSERT INTO users (id, username)
                             VALUES (@Id, @Username)
                             ON CONFLICT (id) DO UPDATE
                             SET username = @Username
                             RETURNING id, username
                             """;

        var command = new CommandDefinition(
            query,
            new { Id = userId, Username = username },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<User>(command);
    }

    public async Task<User?> TryGetUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "SELECT id, username FROM users WHERE id = @UserId";

        var command = new CommandDefinition(
            query,
            new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task AddToWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             INSERT INTO watchlist (user_id, movie_id)
                             VALUES (@UserId, @MovieId)
                             ON CONFLICT (user_id, movie_id) DO NOTHING
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task RemoveFromWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "DELETE FROM watchlist WHERE user_id = @UserId AND movie_id = @MovieId";

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<Movie>> GetWatchListAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT 
                                 m.id as Id, 
                                 m.title as Title, 
                                 m.year as Year, 
                                 m.rating as Rating, 
                                 m.overview as Overview, 
                                 m.media_type as MediaType, 
                                 m.poster_path as PosterPath, 
                                 m.page_path as PagePath
                             FROM movies m
                             JOIN watchlist w ON m.id = w.movie_id
                             WHERE w.user_id = @UserId
                             ORDER BY m.title
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<Movie>(command);
    }

    public async Task AddToWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             INSERT INTO watched (user_id, movie_id)
                             VALUES (@UserId, @MovieId)
                             ON CONFLICT (user_id, movie_id) DO NOTHING
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task RemoveFromWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "DELETE FROM watched WHERE user_id = @UserId AND movie_id = @MovieId";

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<Movie>> GetWatchedMoviesAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT 
                                 m.id as Id, 
                                 m.title as Title, 
                                 m.year as Year, 
                                 m.rating as Rating, 
                                 m.overview as Overview, 
                                 m.media_type as MediaType, 
                                 m.poster_path as PosterPath, 
                                 m.page_path as PagePath,
                                 w.user_rating AS UserRating
                             FROM movies m
                             JOIN watched w ON m.id = w.movie_id
                             WHERE w.user_id = @UserId
                             ORDER BY m.title
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<Movie>(command);
    }

    public async Task SetRatingAsync(long userId, long movieId, int rating,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             UPDATE watched 
                             SET user_rating = @Rating 
                             WHERE user_id = @UserId AND movie_id = @MovieId
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId, Rating = rating },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<bool> IsInWatchlistAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT COUNT(1) FROM watchlist 
                             WHERE user_id = @UserId AND movie_id = @MovieId
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<int?>(command);
        return result > 0;
    }

    public async Task<bool> IsInWatchedAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT COUNT(1) FROM watched 
                             WHERE user_id = @UserId AND movie_id = @MovieId
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<int?>(command);
        return result > 0;
    }

    public async Task<int?> GetRatingAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT user_rating 
                             FROM watched
                             WHERE user_id = @UserId AND movie_id = @MovieId
                             """;

        var command = new CommandDefinition(
            query,
            new { UserId = userId, MovieId = movieId },
            cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<int?>(command);
    }
}