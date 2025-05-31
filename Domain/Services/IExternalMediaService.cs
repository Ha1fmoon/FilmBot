using Domain.Models;

namespace Domain.Services;

public interface IExternalMediaService
{
    Task<SearchResults> SearchMediaAsync(string query, MediaType mediaType, int page,
        CancellationToken cancellationToken);
}