using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Random;

public class Random : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Random");

    public override async Task HandleAsync()
    {
        await SendMessageAsync(Texts.Localization.Get("Messages.RandomChoice"), KeyboardMarkups.RandomSelection());
    }
}