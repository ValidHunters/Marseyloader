using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Marsey.Misc;

namespace Marsey.Stealthsey;

/// <summary>
/// Manages hiding types from the executing assembly
/// </summary>
public static class Facade
{
    private static readonly List<Type> _types = new List<Type>();
    private static readonly List<string> _keywords = new List<string> { "Robust", "Content", "Wizards", "Microsoft", "System" };
    private static Type[] _cached = Array.Empty<Type>();
    
    /// <summary>
    /// Hides type from executing assembly
    /// </summary>
    private static void Imposition(Type patch)
    {
        if (!_types.Contains(patch)) _types.Add(patch);
    }

    /// <summary>
    /// Hides an array of type from patch
    /// </summary>
    private static void Imposition(Type[] patch)
    {
        foreach (Type t in patch)
            Imposition(t);
    }

    /// <summary>
    /// Hides types of a namespace from assembly
    /// </summary>
    public static void Imposition(string name)
    {
        Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type? t in assemblyTypes.Where(t => t.Namespace != null && t.Namespace.StartsWith(name)))
        {
            Imposition(t);
        }
    }

    /// <summary>
    /// Hides all types of assembly from patch
    /// </summary>
    /// <param name="patch"></param>
    public static void Cloak(Assembly patch)
    {
        Type[] typeArray;
        try
        {
            typeArray = patch.GetTypes();
            
            // Hide types if their namespace isn't null and doesn't start with any of the protected keywords
            Imposition(typeArray.Where(type => type.Namespace != null && !_keywords.Any(keyword => type.Namespace.StartsWith(keyword))).ToArray());
        }
        catch (ReflectionTypeLoadException)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"[Facade] {patch.GetName().Name} ({Path.GetFileName(patch.Location)}): Unable to cloak types");
        }
    }

    public static void Cache(Type[] result) => _cached = result;

    public static Type[] Cached => _cached;

    public static IEnumerable<Type> GetTypes() => _types.ToArray();
}