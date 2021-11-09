using System;
using NUnit.Framework;

namespace SS14.Launcher.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class UriHelperTests
{
    [Test]
    [TestCase("server.spacestation14.io", "http://server.spacestation14.io:1212/status")]
    [TestCase("ss14s://server.spacestation14.io", "https://server.spacestation14.io/status")]
    [TestCase("ss14s://server.spacestation14.io:1212", "https://server.spacestation14.io:1212/status")]
    [TestCase("ss14s://server.spacestation14.io/foo", "https://server.spacestation14.io/foo/status")]
    public void GetServerStatusAddress(string input, string expected)
    {
        var uri = UriHelper.GetServerStatusAddress(input);

        Assert.That(uri, Is.EqualTo(new Uri(expected)));
    }
}