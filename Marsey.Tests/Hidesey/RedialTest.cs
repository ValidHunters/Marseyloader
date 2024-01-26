using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Marsey.Game;
using Marsey.Game.Managers;
using Marsey.Stealthsey;

namespace Marsey.HideseyTests;

[TestFixture]
public class RedialTest
{
    private Harmony harm;
    private string HarmonyID = "com.test.harmony";
    private int loadCounter = 0;
    private AssemblyLoadEventHandler testDelegate;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        harm = new Harmony(HarmonyID);
        HarmonyManager.Init(harm);
        
        Hidesey.Initialize();
    }
    
    [SetUp]
    public void SetUp()
    {
        testDelegate = (sender, args) => { loadCounter++; };
        AppDomain.CurrentDomain.AssemblyLoad += testDelegate;
    }

    [TearDown]
    public void TearDown()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= testDelegate;
    }

    [Test]
    public void Redial_DisableAssemblyLoadEvent()
    {
        // Call Redial.Disable to disable the callbacks
        Redial.Disable();

        // Attempt to load a new assembly
        LoadTestAssembly();
        
        // Assembly loaded - enable them back
        Redial.Enable();
        
        // Assert
        Assert.That(loadCounter, Is.EqualTo(0), "AssemblyLoad event should not have been fired.");
    }

    [Test]
    public void Redial_RestoreAssemblyLoadEvent()
    {
        // Disable the callbacks
        Redial.Disable();
        
        // Enable them again
        Redial.Enable();

        // Attempt to load a new assembly and check the loadCounter
        LoadTestAssembly();
        
        // Assert
        Assert.That(loadCounter, Is.EqualTo(1), "AssemblyLoad event should have been fired.");
    }

    private void LoadTestAssembly()
    {
        AssemblyName assemblyName = new AssemblyName("TestAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");
        TypeBuilder typeBuilder = moduleBuilder.DefineType("TestType", TypeAttributes.Public);
        TypeInfo typeInfo = typeBuilder.CreateTypeInfo();

        // The assembly is automatically loaded into the current application domain because we specified AssemblyBuilderAccess.Run
    }

}