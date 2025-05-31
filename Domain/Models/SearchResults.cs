namespace Domain.Models;

public class SearchResults
{
    public List<Movie> Items { get; set; } = [];
    public string Query { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalResults { get; set; } = 0;
}