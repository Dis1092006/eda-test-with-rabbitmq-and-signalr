using SignalRHub.Services;

namespace SignalRHub.Tests;

public class FilterServiceTests
{
    private readonly FilterService _sut = new();

    [Fact]
    public void GetConnectionGroup_ReturnsNull_WhenNoSubscription()
    {
        Assert.Null(_sut.GetConnectionGroup("conn1"));
    }

    [Fact]
    public void TrackSubscription_StoresGroup_ForConnection()
    {
        _sut.TrackSubscription("conn1", null, "error");

        Assert.Equal("error", _sut.GetConnectionGroup("conn1"));
    }

    [Fact]
    public void TrackSubscription_OverwritesGroup_WhenSwitchingFilter()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.TrackSubscription("conn1", "error", "info");

        Assert.Equal("info", _sut.GetConnectionGroup("conn1"));
    }

    [Fact]
    public void RemoveConnection_ClearsGroupForConnection()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.RemoveConnection("conn1");

        Assert.Null(_sut.GetConnectionGroup("conn1"));
    }

    [Fact]
    public void GetMatchingGroups_ReturnsGroup_WhenMessageContainsFilter()
    {
        _sut.TrackSubscription("conn1", null, "error");

        var result = _sut.GetMatchingGroups("error occurred");

        Assert.Contains("error", result);
    }

    [Fact]
    public void GetMatchingGroups_DoesNotReturnGroup_WhenMessageDoesNotContainFilter()
    {
        _sut.TrackSubscription("conn1", null, "error");

        var result = _sut.GetMatchingGroups("hello world");

        Assert.DoesNotContain("error", result);
    }

    [Fact]
    public void GetMatchingGroups_IsCaseInsensitive()
    {
        _sut.TrackSubscription("conn1", null, "ERROR");

        var result = _sut.GetMatchingGroups("error occurred");

        Assert.Contains("ERROR", result);
    }

    [Fact]
    public void GetMatchingGroups_ReturnsMultipleGroups_WhenMessageMatchesSeveral()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.TrackSubscription("conn2", null, "info");

        var result = _sut.GetMatchingGroups("error info message").ToList();

        Assert.Contains("error", result);
        Assert.Contains("info", result);
    }

    [Fact]
    public void GetMatchingGroups_GroupRemainsActive_WhenOneOfTwoConnectionsLeaves()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.TrackSubscription("conn2", null, "error");

        _sut.RemoveConnection("conn1");

        Assert.Contains("error", _sut.GetMatchingGroups("error occurred"));
    }

    [Fact]
    public void GetMatchingGroups_GroupDisappears_WhenAllConnectionsLeave()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.TrackSubscription("conn2", null, "error");

        _sut.RemoveConnection("conn1");
        _sut.RemoveConnection("conn2");

        Assert.DoesNotContain("error", _sut.GetMatchingGroups("error occurred"));
    }

    [Fact]
    public void TrackSubscription_OldGroupDisappears_WhenLastConnectionSwitches()
    {
        _sut.TrackSubscription("conn1", null, "error");
        _sut.TrackSubscription("conn1", "error", "info");

        Assert.DoesNotContain("error", _sut.GetMatchingGroups("error occurred"));
        Assert.Contains("info", _sut.GetMatchingGroups("info message"));
    }
}
