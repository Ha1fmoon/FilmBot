namespace Domain.Models;

public class Movie
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public double Rating { get; set; }
    public string Overview { get; set; } = string.Empty;
    public string PosterPath { get; set; } = string.Empty;
    public string PagePath { get; set; } = string.Empty;
    public MediaType MediaType { get; set; } = MediaType.Movie;
    public int? UserRating { get; set; }
}