using Dapper;
using Domain.Models;
using Domain.Repositories;
using Npgsql;

namespace Infrastructure.LocalRepositories.FilmLibraryDataBase;

public class DapperMovieRepository : IMovieRepository
{
    private readonly string _connectionString;

    public DapperMovieRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Movie?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             SELECT id AS Id,
                                    title AS Title,
                                    year AS Year,
                                    rating AS Rating,
                                    overview AS Overview,
                                    media_type AS MediaType,
                                    poster_path AS PosterPath,
                                    page_path AS PagePath
                             FROM movies 
                             WHERE id = @id
                             """;

        var command = new CommandDefinition(
            query,
            new { id },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Movie>(command);
    }

    public async Task AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = """
                             INSERT INTO movies 
                             (
                                id, title, year, rating, overview, 
                                media_type, poster_path, page_path
                             ) 
                             VALUES 
                             (
                                @Id, @Title, @Year, @Rating, @Overview, 
                                @MediaType, @PosterPath, @PagePath
                             )
                             ON CONFLICT (id) DO NOTHING
                             """;

        var parameters = new
        {
            movie.Id,
            movie.Title,
            movie.Year,
            movie.Rating,
            movie.Overview,
            movie.MediaType,
            movie.PosterPath,
            movie.PagePath
        };

        var command = new CommandDefinition(
            query,
            parameters,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<bool> ExistsAsync(long movieId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "SELECT COUNT(1) FROM movies WHERE id = @Id";

        var command = new CommandDefinition(
            query,
            new { Id = movieId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<int>(command);

        return result > 0;
    }
}