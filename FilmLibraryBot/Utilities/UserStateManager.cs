using System.Collections.Concurrent;

namespace FilmLibraryBot.Utilities;

public class UserStateManager
{
    private readonly ConcurrentDictionary<long, (string State, object? Data)> _states = new();

    public void SetState(long userId, string state, object? data = null)
    {
        _states[userId] = (state, data);
    }

    public bool TryGetState(long userId, out string state, out object? data)
    {
        if (_states.TryGetValue(userId, out var stateData))
        {
            state = stateData.State;
            data = stateData.Data;
            return true;
        }

        state = string.Empty;
        data = null;
        return false;
    }

    public void ClearState(long userId)
    {
        _states.TryRemove(userId, out _);
    }
}