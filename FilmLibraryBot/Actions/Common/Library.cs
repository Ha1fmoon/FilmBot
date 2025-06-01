using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Common;

public class Library : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Library");

    public override async Task HandleAsync()
    {
        await SendMessageAsync(
            Texts.Localization.Get("Messages.ChooseLibraryType"), 
            KeyboardMarkups.LibraryTypeSelection()
        );
    }

}