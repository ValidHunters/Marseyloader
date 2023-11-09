using System;
using System.Reflection;

namespace Marsey;

public abstract class Utility
{
    public enum LogType
    {
        INFO,
        WARN,
        FATL,
        DEBG
    }

    // Loader logs
    public static void Log(LogType logType, string message)
    {
        Console.WriteLine($"[MARSEY] [{logType.ToString()}] {message}");
    }

    // Patch logs
    public static void Log(AssemblyName asm, string message)
    {
        Console.WriteLine($"[{asm.Name}] {message}");
    }

    /// <summary>
    /// Sets patch delegate to Utility::Log(AssemblyName, string)
    /// Executed only by the Loader.
    /// </summary>
    /// <see cref="PatchAssemblyManager.InitLogger"/>
    /// <param name="patch">Assembly from MarseyPatch</param>
    public static void SetupLogger(Assembly patch)
    {
        Type marseyLoggerType = patch.GetType("MarseyLogger")!;

        Type logDelegateType = marseyLoggerType.GetNestedType("Forward", BindingFlags.Public)!;

        MethodInfo logMethod = typeof(Utility).GetMethod("Log", new []{typeof(AssemblyName), typeof(string)})!;

        Delegate logDelegate = Delegate.CreateDelegate(logDelegateType, null, logMethod);

        marseyLoggerType.GetField("logDelegate", BindingFlags.Public | BindingFlags.Static).SetValue(null, logDelegate);
    }
}
