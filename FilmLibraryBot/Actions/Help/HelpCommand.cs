using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Help;

public class Help : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Help");
    public override bool IsCommandListRequired => true;

    public override async Task HandleAsync()
    {
        await SendMessageAsync($"{Texts.Localization.Get("Messages.Help")}:\n{CommandsList}");
    }
}