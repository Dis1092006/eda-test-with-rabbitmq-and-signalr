using System.Collections.Concurrent;

namespace SignalRHub.Services;

public class FilterService
{
    // Which SignalR group each connection is currently in (to leave on switch/disconnect)
    private readonly ConcurrentDictionary<string, string> _connectionGroups = new();

    // How many active connections are in each group (group name → subscriber count)
    private readonly ConcurrentDictionary<string, int> _activeGroups = new();

    public string? GetConnectionGroup(string connectionId) =>
        _connectionGroups.TryGetValue(connectionId, out var group) ? group : null;

    public void TrackSubscription(string connectionId, string? oldGroup, string newGroup)
    {
        _connectionGroups[connectionId] = newGroup;
        _activeGroups.AddOrUpdate(newGroup, 1, (_, count) => count + 1);

        if (oldGroup != null)
            DecrementGroup(oldGroup);
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connectionGroups.TryRemove(connectionId, out var group))
            DecrementGroup(group);
    }

    public IEnumerable<string> GetMatchingGroups(string message) =>
        _activeGroups.Keys
            .Where(g => message.Contains(g, StringComparison.OrdinalIgnoreCase));

    private void DecrementGroup(string group)
    {
        _activeGroups.AddOrUpdate(group, 0, (_, count) => count - 1);
        if (_activeGroups.TryGetValue(group, out var count) && count <= 0)
            _activeGroups.TryRemove(group, out _);
    }
}
