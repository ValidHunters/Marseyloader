using System;
using NUnit.Framework;
using SS14.Launcher.Models;

namespace SS14.Launcher.Tests
{
    [TestFixture]
    public class UriHelperTests
    {
        [Test]
        public void GetServerStatusAddress()
        {
            var uri = UriHelper.GetServerStatusAddress("server.spacestation14.io");

            Assert.AreEqual(new Uri("http://server.spacestation14.io:1212/status"), uri);

            uri = UriHelper.GetServerStatusAddress("ss14s://server.spacestation14.io");

            Assert.AreEqual(new Uri("https://server.spacestation14.io/status"), uri);

            uri = UriHelper.GetServerStatusAddress("ss14s://server.spacestation14.io:1212");

            Assert.AreEqual(new Uri("https://server.spacestation14.io:1212/status"), uri);

            uri = UriHelper.GetServerStatusAddress("ss14s://server.spacestation14.io/foo");

            Assert.AreEqual(new Uri("https://server.spacestation14.io/foo/status"), uri);
        }
    }
}