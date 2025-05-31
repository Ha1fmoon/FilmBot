using System.Reflection;
using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Handlers;

public class CommandHandler
{
    private readonly Dictionary<string, ICommand> _commands = [];
    private readonly IErrorLogger _errorLogger;
    private readonly IMovieService _movieService;
    private readonly IUserService _userService;
    private readonly UserStateManager _userStateManager;

    public CommandHandler(UserStateManager userStateManager, IMovieService movieService, IUserService userService,
        IErrorLogger errorLogger)
    {
        _userStateManager = userStateManager;
        _movieService = movieService;
        _userService = userService;
        _errorLogger = errorLogger;
    }

    private void RegisterCommand(ICommand command)
    {
        if (command.IsStateRequired) command.SetStateManager(_userStateManager);
        if (command.IsMovieServiceRequired) command.SetMovieService(_movieService);
        if (command.IsUserServiceRequired) command.SetUserService(_userService);

        command.SetErrorLogger(_errorLogger);

        _commands[command.Name.ToLower()] = command;
    }

    public bool TryGetCommand(string name, out ICommand? command)
    {
        name = name.TrimStart('/').ToLower();

        return _commands.TryGetValue(name, out command);
    }

    public IEnumerable<ICommand> GetAllCommands()
    {
        return _commands.Values;
    }

    private string GetFormattedCommandsList()
    {
        var commandsList = GetAllCommands()
            .OrderBy(c => c.Name)
            .Select(c => $"/{c.Name} - {c.Description}")
            .ToList();

        return string.Join("\n", commandsList);
    }

    public void RegisterCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();

        var listRequiredCommands = new List<ICommand>();

        foreach (var type in commandTypes)
            try
            {
                var command = (ICommand)Activator.CreateInstance(type)!;

                if (command.IsCommandListRequired) listRequiredCommands.Add(command);

                RegisterCommand(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Texts.Localization.Get("Messages.CommandRegistrationError", type.Name, ex.Message));
            }

        var formattedList = GetFormattedCommandsList();

        foreach (var command in listRequiredCommands) command.SetCommandsList(formattedList);
    }
}