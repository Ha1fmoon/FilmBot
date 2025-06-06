﻿using System.Reflection;
using Domain.Exceptions;
using Domain.Services;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FilmLibraryBot.Handlers;

public class CallbackQueryHandler
{
    private readonly Dictionary<string, Type> _callbackTypes = new();
    private readonly IErrorLogger _errorLogger;
    private readonly IMovieService _movieService;
    private readonly IUserService _userService;
    private readonly UserStateManager _userStateManager;

    public CallbackQueryHandler(UserStateManager userStateManager, IMovieService movieService, IUserService userService,
        IErrorLogger errorLogger)
    {
        _userStateManager = userStateManager;
        _movieService = movieService;
        _userService = userService;
        _errorLogger = errorLogger;
    }

    public async Task RegisterCallbacksFromAssembly(Assembly assembly)
    {
        var callbackTypes = assembly.GetTypes()
            .Where(t => typeof(ICallback).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var type in callbackTypes)
            try
            {
                var callback = (ICallback)Activator.CreateInstance(type)!;
                _callbackTypes[callback.Name.ToLower()] = type;
            }
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex, $"CallbackQueryHandler.RegisterCallback - {type.Name}");
            }
    }

    public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data;

        if (string.IsNullOrEmpty(data))
            return;

        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        if (await HandleSystemCallbacks(botClient, callbackQuery, data, cancellationToken))
            return;

        if (!TryParseCallbackData(data, out var callbackName, out var callbackValue))
            return;

        await ExecuteCallbackHandler(botClient, callbackQuery, callbackName, callbackValue, cancellationToken);
    }

    private async Task<bool> HandleSystemCallbacks(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        string data, CancellationToken cancellationToken)
    {
        if (data != "cancel" && data != "back")
            return false;

        _userStateManager.ClearState(callbackQuery.From.Id);

        await EditCallbackMessage(
            botClient,
            callbackQuery,
            Texts.Localization.Get("Messages.ActionCancelled"),
            cancellationToken
        );

        return true;
    }

    private bool TryParseCallbackData(string data, out string name, out string value)
    {
        value = string.Empty;

        var separatorIndex = data.IndexOf(':');
        if (separatorIndex != -1)
        {
            name = data.Substring(0, separatorIndex);
            value = data.Substring(separatorIndex + 1);
        }
        else
        {
            name = data;
        }

        return true;
    }

    private async Task ExecuteCallbackHandler(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        string callbackName, string callbackValue, CancellationToken cancellationToken)
    {
        if (_callbackTypes.TryGetValue(callbackName.ToLower(), out var callbackType))
            try
            {
                var callback = CreateCallbackInstance(callbackType);
                await callback.ExecuteAsync(botClient, callbackQuery, callbackValue, cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleCallbackError(botClient, callbackQuery, ex, cancellationToken);
            }
        else
            await HandleCallbackNotFound(botClient, callbackQuery, callbackName, cancellationToken);
    }

    private ICallback CreateCallbackInstance(Type callbackType)
    {
        var callback = (ICallback)Activator.CreateInstance(callbackType)!;

        if (callback.IsMovieServiceRequired)
            callback.SetMovieService(_movieService);

        if (callback.IsUserServiceRequired)
            callback.SetUserService(_userService);

        if (callback.IsStateRequired)
            callback.SetStateManager(_userStateManager);

        callback.SetErrorLogger(_errorLogger);

        return callback;
    }

    private async Task HandleCallbackError(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        Exception exception, CancellationToken cancellationToken)
    {
        await _errorLogger.LogErrorAsync(exception, "CallbackQueryHandler.HandleCallbackError");

        await EditCallbackMessage(
            botClient,
            callbackQuery,
            Texts.Localization.Get("Messages.CallbackError"),
            cancellationToken
        );
    }

    private async Task HandleCallbackNotFound(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        string callbackName, CancellationToken cancellationToken)
    {
        await _errorLogger.LogWarningAsync(Texts.Localization.Get("Messages.CallbackHandlerNotFound", callbackName),
            "CallbackQueryHandler.HandleCallbackNotFound");

        await EditCallbackMessage(
            botClient,
            callbackQuery,
            Texts.Localization.Get("Messages.CallbackNotFound"),
            cancellationToken
        );
    }

    private async Task EditCallbackMessage(ITelegramBotClient botClient, CallbackQuery callbackQuery, string text,
        CancellationToken cancellationToken)
    {
        if (callbackQuery.Message != null)
            await botClient.EditMessageText(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                text,
                cancellationToken: cancellationToken
            );
    }
}