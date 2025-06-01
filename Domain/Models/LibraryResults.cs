namespace Domain.Models;

public class LibraryResults
{
    public const int ItemsPerPage = 20;
    public List<Movie> Items { get; set; } = [];
    public LibraryType LibraryType { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalResults { get; set; } = 0;
    public long UserId { get; set; }
}