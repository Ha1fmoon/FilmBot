using Application.Services;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services;
using DotNetEnv;
using FilmLibraryBot.Handlers;
using FilmLibraryBot.Services;
using FilmLibraryBot.Utilities;
using Infrastructure.ExternalServices.TMDB;
using Infrastructure.LocalRepositories.FilmLibraryDataBase;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FilmLibraryBot;

internal class Program
{
    private static async Task Main()
    {
        var cts = new CancellationTokenSource();

        Env.Load();

        InitializeRepositoriesAndServices(out var userService, out var movieService);

        var botClient = new TelegramBotClient(Configuration.TelegramToken);
        var updateHandler = await InitializeBotHandlersAsync(movieService, userService, botClient, cts.Token);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true
        };

        botClient.StartReceiving(updateHandler, receiverOptions, cts.Token);

        await Task.Run(() => HandleInput(cts), cts.Token);
    }

    private static async Task<UpdateHandler> InitializeBotHandlersAsync(IMovieService movieService,
        IUserService userService, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var stateManager = new UserStateManager();
        IErrorLogger errorLogger = new ConsoleLogger();

        var commandHandler = new CommandHandler(stateManager, movieService, userService, errorLogger);
        await commandHandler.RegisterCommandsFromAssembly(typeof(Program).Assembly);

        var callbackQueryHandler = new CallbackQueryHandler(stateManager, movieService, userService, errorLogger);
        await callbackQueryHandler.RegisterCallbacksFromAssembly(typeof(Program).Assembly);

        var textHandler = new TextHandler(stateManager, movieService);
        var updateHandler = new UpdateHandler(commandHandler, callbackQueryHandler, textHandler, errorLogger);

        await SetBotCommandsAsync(botClient, commandHandler, cancellationToken);

        return updateHandler;
    }

    private static void InitializeRepositoriesAndServices(out IUserService userService, out IMovieService movieService)
    {
        IErrorLogger errorLogger = new ConsoleLogger();

        try
        {
            IMovieRepository movieRepository = new DapperMovieRepository(Configuration.DbConnectionString);
            IUserRepository userRepository = new DapperUserRepository(Configuration.DbConnectionString);

            if (movieRepository == null)
                throw new InvalidOperationException("Failed to initialize movie repository");

            if (userRepository == null)
                throw new InvalidOperationException("Failed to initialize user repository");

            IExternalMediaService external =
                new TmdbMediaService(Configuration.TmdbApiKey, errorLogger, Configuration.TmdbLanguage);

            movieService = new MovieService(movieRepository, external, errorLogger);
            userService = new UserService(userRepository, errorLogger);

            if (movieService == null)
                throw new InvalidOperationException("Failed to initialize movie service");

            if (userService == null)
                throw new InvalidOperationException("Failed to initialize user service");

            var localizationService = new LocalizationService(errorLogger, Configuration.BotLanguage);
            Texts.Initialize(localizationService);
        }
        catch (Exception ex)
        {
            errorLogger.LogErrorAsync(ex, "Program.InitializeRepositoriesAndServices").Wait();
            throw;
        }
    }

    private static async Task SetBotCommandsAsync(ITelegramBotClient botClient, CommandHandler commandHandler,
        CancellationToken cancellationToken)
    {
        var commands = commandHandler.GetAllCommands()
            .Where(c => !string.IsNullOrEmpty(c.Description))
            .Select(c => new BotCommand
            {
                Command = c.Name.ToLower(),
                Description = c.Description
            })
            .ToList();

        await botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
    }

    private static async Task HandleInput(CancellationTokenSource cts)
    {
        Console.WriteLine("Press 'A' to quit.");
        while (!cts.Token.IsCancellationRequested)
        {
            var key = Console.ReadKey(true).Key;

            if (key != ConsoleKey.A) continue;

            Console.WriteLine("Quitting...");
            await cts.CancelAsync();
            break;
        }
    }
}