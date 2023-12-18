using NUnit.Framework;
using Marsey.Stealthsey;
using Marsey.Stealthsey.Reflection;
using System.Reflection;
using Marsey.Config;

namespace Marsey.HideseyTests;

[TestFixture]
public class HideLevelTests
{
    [Test]
    public void HideLevelRequirement_Executes_AtRequiredLevel()
    {
        // Arrange
        MarseyVars.MarseyHide = HideLevel.Normal; // Set the current HideLevel to Normal
        MethodInfo method = typeof(TestClass).GetMethod("MethodWithRequirement")!;

        // Act
        bool canExecute = HideseyPatches.LevelCheck(method);

        // Assert
        Assert.That(canExecute, Is.True, "Method should execute at required HideLevel.");
    }

    [Test]
    public void HideLevelRequirement_Blocked_BelowRequiredLevel()
    {
        // Arrange
        MarseyVars.MarseyHide = HideLevel.Disabled; // Set the current HideLevel below the requirement
        MethodInfo method = typeof(TestClass).GetMethod("MethodWithRequirement")!;

        // Act
        bool canExecute = HideseyPatches.LevelCheck(method);

        // Assert
        Assert.That(canExecute, Is.False, "Method should be blocked below required HideLevel.");
    }

    [Test]
    public void HideLevelRestriction_Executes_BelowMaxLevel()
    {
        // Arrange
        MarseyVars.MarseyHide = HideLevel.Normal; // Set the current HideLevel below the maximum
        MethodInfo method = typeof(TestClass).GetMethod("MethodWithRestriction")!;

        // Act
        bool canExecute = HideseyPatches.LevelCheck(method);

        // Assert
        Assert.That(canExecute, Is.True, "Method should execute below maximum HideLevel.");
    }

    [Test]
    public void HideLevelRestriction_Blocked_AtOrAboveMaxLevel()
    {
        // Arrange
        MarseyVars.MarseyHide = HideLevel.Unconditional; // Set the current HideLevel at maximum
        MethodInfo method = typeof(TestClass).GetMethod("MethodWithRestriction")!;

        // Act
        bool canExecute = HideseyPatches.LevelCheck(method);

        // Assert
        Assert.That(canExecute, Is.False, "Method should be blocked at or above maximum HideLevel.");
    }
}

public class TestClass
{
    [HideLevelRequirement(HideLevel.Normal)]
    public void MethodWithRequirement()
    {
        Console.WriteLine("Test");
    }

    [HideLevelRestriction(HideLevel.Explicit)]
    public void MethodWithRestriction()
    {
        Console.WriteLine("Test");
    }
}
