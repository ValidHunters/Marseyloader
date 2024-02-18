using System;
namespace Marsey.Stealthsey.Reflection;

/// <summary>
/// Restricts method execution if HideLevel is below
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class HideLevelRequirement : Attribute
{
    public HideLevel Level { get; }

    public HideLevelRequirement(HideLevel level)
    {
        Level = level;
    }
}

/// <summary>
/// Restricts method execution if HideLevel is above or equal
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class HideLevelRestriction : Attribute
{
    public HideLevel MaxLevel { get; }

    public HideLevelRestriction(HideLevel maxLevel)
    {
        MaxLevel = maxLevel;
    }
}
