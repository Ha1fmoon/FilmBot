using FilmLibraryBot.Services;

namespace FilmLibraryBot.Utilities;

public static class Texts
{
    public static LocalizationService Localization { get; private set; } = null!;

    public static void Initialize(LocalizationService localization)
    {
        Localization = localization;
    }
}