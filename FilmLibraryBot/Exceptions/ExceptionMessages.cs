namespace FilmLibraryBot.Exceptions;

public static class ExceptionMessages
{
    public const string MovieServiceRequired =
        "To use MovieService, it must be enabled with IsMovieServiceRequired";

    public const string UserServiceRequired =
        "To use UserService, it must be enabled with IsUserServiceRequired";

    public const string StateManagerRequired =
        "To use StateManager, it must be enabled with IsStateRequired";

    public const string CommandListRequired =
        "To use CommandsList, it must be enabled with IsCommandListRequired";

    public const string MovieServiceNotInitialized = "MovieService is not initialized";
    public const string UserServiceNotInitialized = "UserService is not initialized";
    public const string StateManagerNotInitialized = "StateManager is not initialized";
    public const string CommandsListNotInitialized = "CommandsList is not initialized";
}