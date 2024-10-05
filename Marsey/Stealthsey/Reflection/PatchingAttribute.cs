using System.Reflection;

namespace Marsey.Stealthsey.Reflection;

/// <summary>
/// Method is patching the game in some form
/// <remarks> Does not execute if <see cref="Patching"/> is true </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class Patching : Attribute;
