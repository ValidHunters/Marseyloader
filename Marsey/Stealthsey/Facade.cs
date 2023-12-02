using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Marsey.Stealthsey;

public static class Facade
{
    private static List<Type> _types = new List<Type>();
    private static List<string> _keywords = new List<string> { "Robust", "Content", "Wizards", "Microsoft", "System" };
    
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
    public static void Imposition(Type[] patch)
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
        foreach (Type t in assemblyTypes.Where(t => t.Namespace!.StartsWith(name)))
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
            Imposition(typeArray.Where(type => type.Namespace != null && !_keywords.Any(keyword => type.Namespace.StartsWith(keyword))).ToArray());
        }
        catch (ReflectionTypeLoadException)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"[Facade] {patch.GetName().Name} ({Path.GetFileName(patch.Location)}): Unable to cloak types");
        }
    }


    public static Type[] GetTypes() => _types.ToArray();
}