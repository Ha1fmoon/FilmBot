﻿using Domain.Exceptions;
using Domain.Models;
using Domain.Repositories;
using Domain.Services;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IErrorLogger _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, IErrorLogger logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> GetOrCreateUserAsync(long userId, string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.TryGetUserAsync(userId, cancellationToken);

            if (user == null || user.Username != username)
                user = await _userRepository.TrySaveUserAsync(userId, username, cancellationToken);

            return user;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.GetOrCreateUserAsync: userId={userId}, username={username}");
            return null;
        }
    }

    public async Task AddToWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userRepository.AddToWatchListAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.AddToWatchListAsync: userId={userId}, movieId={movieId}");
        }
    }

    public async Task RemoveFromWatchListAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userRepository.RemoveFromWatchListAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"UserService.RemoveFromWatchListAsync: userId={userId}, movieId={movieId}");
        }
    }

    public async Task<LibraryResults> GetWatchListAsync(long userId, int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetWatchListAsync(userId, page, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.GetWatchListAsync: userId={userId}");
            return new LibraryResults();
        }
    }

    public async Task MarkAsWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userRepository.RemoveFromWatchListAsync(userId, movieId, cancellationToken);
            await _userRepository.AddToWatchedAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.MarkAsWatchedAsync: userId={userId}, movieId={movieId}");
        }
    }

    public async Task RemoveFromWatchedAsync(long userId, long movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userRepository.RemoveFromWatchedAsync(userId, movieId, cancellationToken);
            await _userRepository.AddToWatchListAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.RemoveFromWatchedAsync: userId={userId}, movieId={movieId}");
        }
    }

    public async Task<LibraryResults> GetWatchedMoviesAsync(long userId, int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetWatchedMoviesAsync(userId, page, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.GetWatchedMoviesAsync: userId={userId}");
            return new LibraryResults();
        }
    }

    public async Task<Movie?> GetRandomFromWatchListAsync(long userId, MediaType? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var watchList = (await _userRepository.GetFullWatchListAsync(userId, cancellationToken)).ToList();

            if (watchList.Count == 0) return null;

            var filteredList = mediaType.HasValue
                ? watchList.Where(m => m.MediaType == mediaType.Value).ToList()
                : watchList;

            if (filteredList.Count == 0) return null;

            var randomIndex = new Random().Next(filteredList.Count);
            return filteredList[randomIndex];
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"UserService.GetRandomFromWatchListAsync: userId={userId}, mediaType={mediaType}");
            return null;
        }
    }

    public async Task RateMovieAsync(long userId, long movieId, int rating,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (rating < 1 || rating > 10)
            {
                await _logger.LogWarningAsync($"Attempted to set invalid rating: {rating}",
                    $"UserService.RateMovieAsync: userId={userId}, movieId={movieId}");
                return;
            }

            await _userRepository.SetRatingAsync(userId, movieId, rating, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex,
                $"UserService.RateMovieAsync: userId={userId}, movieId={movieId}, rating={rating}");
        }
    }

    public async Task<bool> IsInWatchlistAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        try
        {
            return await _userRepository.IsInWatchlistAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.IsInWatchlistAsync: userId={userId}, movieId={movieId}");
            return false;
        }
    }

    public async Task<bool> IsInWatchedAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        try
        {
            return await _userRepository.IsInWatchedAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.IsInWatchedAsync: userId={userId}, movieId={movieId}");
            return false;
        }
    }

    public async Task<int?> GetRatingAsync(long userId, long movieId, CancellationToken cancellationToken)
    {
        try
        {
            return await _userRepository.GetRatingAsync(userId, movieId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, $"UserService.GetRatingAsync: userId={userId}, movieId={movieId}");
            return null;
        }
    }
}