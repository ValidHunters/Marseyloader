using System.Reflection;
using HarmonyLib;
using Marsey.Game.Managers;
using Marsey.Stealthsey;

namespace Marsey.HideseyTests;

[TestFixture]
public class HideseyTest
{
    private string HarmonyID = "com.marsey.tests"; 
    private Harmony harm;
        
    [OneTimeSetUp]
    public void SetUp()
    {
        harm = new Harmony(HarmonyID);
        HarmonyManager.Init(harm);
        Hidesey.Initialize();
    }

    [Test]
    public void Hidesey_HiddenAssemblies()
    {
        // Arrange
        Hidesey.Disperse(); // By the end of marseypatcher this is run once more because Marsey gets re-inited
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<string> HiddenAssemblies = new List<string> { "Harmony", "Marsey", "MonoMod", "Mono.", "System.Reflection.Emit," };
        
        // Filter assemblies that match the fullname in HiddenAssemblies
        Assembly[] filteredAssemblies = assemblies.Where(assembly => 
            HiddenAssemblies.Any(forbidden => assembly.FullName != null && assembly.FullName.Contains(forbidden))
        ).ToArray();

        // Assert that there should be no assemblies found 
        Assert.IsEmpty(filteredAssemblies, "Forbidden assemblies were found in the domain.");
    }
    
    [Test]
    public void Hidesey_HiddenTypes()
    {
        // Arrange
        List<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .ToList();
        
        List<string> HiddenTypeNamespaces = new List<string> { "Marsey", "Harmony" };
        
        // Act
        Type[] filteredTypes = allTypes.Where(type => 
            HiddenTypeNamespaces.Any(forbidden => type.Namespace?.StartsWith(forbidden) ?? false)
        ).ToArray();
        
        // Assert
        Assert.IsEmpty(filteredTypes, "Forbidden types were found in the domain.");
    }
}