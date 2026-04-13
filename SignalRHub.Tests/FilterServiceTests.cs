using SignalRHub.Services;

namespace SignalRHub.Tests;

public class FilterServiceTests
{
    private readonly FilterService _sut = new();

    [Fact]
    public void GetMatchingConnections_ReturnsConnection_WhenMessageContainsFilter()
    {
        _sut.SetFilter("conn1", "hello");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.Contains("conn1", result);
    }

    [Fact]
    public void GetMatchingConnections_DoesNotReturnConnection_WhenMessageDoesNotContainFilter()
    {
        _sut.SetFilter("conn1", "xyz");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.DoesNotContain("conn1", result);
    }

    [Fact]
    public void GetMatchingConnections_IsCaseInsensitive()
    {
        _sut.SetFilter("conn1", "HELLO");

        var result = _sut.GetMatchingConnections("hello world");

        Assert.Contains("conn1", result);
    }

    [Fact]
    public void RemoveFilter_RemovesConnection()
    {
        _sut.SetFilter("conn1", "hello");
        _sut.RemoveFilter("conn1");

        var result = _sut.GetMatchingConnections("Hello World");

        Assert.DoesNotContain("conn1", result);
    }

    [Fact]
    public void SetFilter_OverwritesPreviousFilter()
    {
        _sut.SetFilter("conn1", "hello");
        _sut.SetFilter("conn1", "world");

        var matchesOld = _sut.GetMatchingConnections("hello test");
        var matchesNew = _sut.GetMatchingConnections("world test");

        Assert.DoesNotContain("conn1", matchesOld);
        Assert.Contains("conn1", matchesNew);
    }

    [Fact]
    public void GetMatchingConnections_ReturnsMultipleConnections_WhenMultipleMatch()
    {
        _sut.SetFilter("conn1", "error");
        _sut.SetFilter("conn2", "error");
        _sut.SetFilter("conn3", "info");

        var result = _sut.GetMatchingConnections("error occurred").ToList();

        Assert.Contains("conn1", result);
        Assert.Contains("conn2", result);
        Assert.DoesNotContain("conn3", result);
    }
}
