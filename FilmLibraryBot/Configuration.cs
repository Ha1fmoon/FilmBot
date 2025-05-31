namespace FilmLibraryBot;

public static class Configuration
{
    public static string TelegramToken =>
        Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ??
        throw new Exception("TELEGRAM_TOKEN not found in environment variables");

    public static string TmdbApiKey =>
        Environment.GetEnvironmentVariable("TMDB_API_KEY") ??
        throw new Exception("TMDB_API_KEY not found in environment variables");

    public static string DbConnectionString =>
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
        throw new Exception("DB_CONNECTION_STRING not found in environment variables");

    public static string BotLanguage => Environment.GetEnvironmentVariable("BOT_LANGUAGE") ?? "en";
    public static string TmdbLanguage => Environment.GetEnvironmentVariable("TMDB_LANGUAGE") ?? "en-EN";
}