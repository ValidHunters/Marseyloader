using Marsey.Stealthsey;
namespace Marsey.HideseyTests;

[TestFixture]
public class EnvseyTest
{
    private const string TestEnvVar = "TEST_ENV_VAR";

    [SetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable(TestEnvVar, "test_value");
    }

    [Test]
    public void CleanFlag_SetEnvVarToNull()
    {
        Envsey.CleanFlag(TestEnvVar);
        
        string? result = Environment.GetEnvironmentVariable(TestEnvVar);
        Assert.That(result, Is.Null, $"The environment variable {TestEnvVar} should be null after calling CleanFlag.");
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(TestEnvVar, null);
    }
}