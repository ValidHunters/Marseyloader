using NUnit.Framework;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[TestOf(typeof(RidUtility))]
public sealed class RidUtilityTest
{
    [Test]
    [TestCase(new[] {"win-x64", "win7"}, "win7-x64", "win7")]
    [TestCase(new[] {"win-x64", "win"}, "win7-x64", "win-x64")]
    public void TestFindBestRid(string[] rids, string start, string expected)
    {
        Assert.That(RidUtility.FindBestRid(rids, start), Is.EqualTo(expected));
    }
}