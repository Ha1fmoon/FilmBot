using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Start;

public class Start : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Start");
    public override bool IsCommandListRequired => true;
    public override bool IsUserServiceRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var message = GetMessage();
        var userName = string.Empty;

        if (message.From != null)
        {
            if (!string.IsNullOrWhiteSpace(message.From.FirstName) ||
                !string.IsNullOrWhiteSpace(message.From.LastName))
                userName = string.Join(" ", message.From.FirstName, message.From.LastName).Trim();
            else if
                (!string.IsNullOrWhiteSpace(message.From.Username))
                userName = message.From.Username;
        }

        await UserService.GetOrCreateUserAsync(userId, userName);

        await SendMessageAsync(
            $"{Texts.Localization.Get("Messages.Greetings")}, {userName}! {Texts.Localization.Get("Messages.Help")}\n{CommandsList}");
    }
}