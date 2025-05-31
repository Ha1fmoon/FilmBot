using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Search;

public class Search : CommandBase
{
    public override string Description => Texts.Localization.Get("CommandDescriptions.Search");

    public override async Task HandleAsync()
    {
        await SendMessageAsync(
            Texts.Localization.Get("Messages.SearchFilter"),
            KeyboardMarkups.MediaTypeSelection()
        );
    }
}