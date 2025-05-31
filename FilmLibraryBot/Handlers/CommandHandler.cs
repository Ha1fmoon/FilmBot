using System.Reflection;
using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Handlers;

public class CommandHandler
{
    private readonly List<(string Name, string Description)> _commandList = [];
    private readonly Dictionary<string, Type> _commandTypes = [];
    private readonly IErrorLogger _errorLogger;
    private readonly IMovieService _movieService;
    private readonly IUserService _userService;
    private readonly UserStateManager _userStateManager;
    private string _formattedCommandsList = string.Empty;

    public CommandHandler(UserStateManager userStateManager, IMovieService movieService, IUserService userService,
        IErrorLogger errorLogger)
    {
        _movieService = movieService;
        _userService = userService;
        _userStateManager = userStateManager;
        _errorLogger = errorLogger;
    }

    public async Task RegisterCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var type in commandTypes)
            try
            {
                var tempCommand = (ICommand)Activator.CreateInstance(type)!;
                _commandTypes[tempCommand.Name.ToLower()] = type;

                _commandList.Add((tempCommand.Name, tempCommand.Description));
            }
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex, $"CommandHandler.RegisterCommand - {type.Name}");
            }

        _formattedCommandsList = string.Join("\n",
            _commandList
                .OrderBy(c => c.Name)
                .Select(c => $"/{c.Name} - {c.Description}")
        );
    }

    public IEnumerable<(string Name, string Description)> GetAllCommands()
    {
        return _commandList.ToList();
    }

    public async Task HandleCommandAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var commandText = message.Text ?? string.Empty;
        var commandName = commandText.TrimStart('/').ToLower();

        if (_commandTypes.TryGetValue(commandName, out var commandType))
            try
            {
                var command = await CreateCommandInstanceAsync(commandType);
                await command.ExecuteAsync(botClient, message, cancellationToken);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex, $"CommandHandler.HandleCommand - {commandType.Name}");

                await botClient.SendMessage(
                    message.Chat.Id,
                    Texts.Localization.Get("Messages.Error"),
                    cancellationToken: cancellationToken
                );
            }
        else
            await HandleCommandNotFound(botClient, message, commandName, cancellationToken);
    }

    private async Task<ICommand> CreateCommandInstanceAsync(Type commandType)
    {
        var command = (ICommand)Activator.CreateInstance(commandType)!;

        if (command.IsMovieServiceRequired)
            command.SetMovieService(_movieService);

        if (command.IsUserServiceRequired)
            command.SetUserService(_userService);

        if (command.IsStateRequired)
            command.SetStateManager(_userStateManager);

        if (command.IsCommandListRequired)
            command.SetCommandsList(_formattedCommandsList);

        command.SetErrorLogger(_errorLogger);

        return command;
    }

    private async Task HandleCommandNotFound(ITelegramBotClient botClient, Message message, string commandName,
        CancellationToken cancellationToken)
    {
        await _errorLogger.LogWarningAsync($"Command not found: {commandName}", "CommandHandler.HandleCommandNotFound");

        await botClient.SendMessage(
            message.Chat.Id,
            Texts.Localization.Get("Messages.UnknownCommand"),
            cancellationToken: cancellationToken
        );
    }
}