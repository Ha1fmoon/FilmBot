using Domain.Models;
using FilmLibraryBot.Base;
using FilmLibraryBot.Utilities;

namespace FilmLibraryBot.Actions.Search;

public class SearchMediaTypeCallback : CallbackBase
{
    public override string Name => "search_media_type";
    public override bool IsStateRequired => true;

    public override async Task HandleAsync()
    {
        var userId = GetUserId();
        var callbackValue = GetValue();

        if (!Enum.TryParse<MediaType>(callbackValue, out var mediaType))
        {
            await EditMessageAsync(Texts.Localization.Get("Messages.InvalidParameter", "MEDIA TYPE"));
            return;
        }

        StateManager.SetState(userId, "WAITING_FOR_MOVIE_NAME", mediaType);

        await EditMessageAsync(Texts.Localization.Get("Messages.EnterSearchQuery"));
    }
}