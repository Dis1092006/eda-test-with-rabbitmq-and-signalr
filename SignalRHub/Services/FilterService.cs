using System.Collections.Concurrent;

namespace SignalRHub.Services;

public class FilterService
{
    private readonly ConcurrentDictionary<string, string> _filters = new();

    public void SetFilter(string connectionId, string filter) =>
        _filters[connectionId] = filter;

    public void RemoveFilter(string connectionId) =>
        _filters.TryRemove(connectionId, out _);

    public IEnumerable<string> GetMatchingConnections(string message) =>
        _filters
            .Where(kv => message.Contains(kv.Value, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key);
}
