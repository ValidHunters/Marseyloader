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

    /// <summary>
    /// Log function used by the loader
    /// </summary>
    /// <param name="logType">Log level</param>
    /// <param name="message">Log message</param>
    public static void Log(LogType logType, string message)
    {
        if (logType == LogType.DEBG && MarseyVars.DebugAllowed != true)
            return;

        Console.WriteLine($"[MARSEY] [{logType.ToString()}] {message}");
    }

    /// <summary>
    /// Log function used by patches
    /// </summary>
    /// <param name="asm">Assembly name of patch</param>
    /// <param name="message">Log message</param>
    public static void Log(AssemblyName asm, string message)
    {
        if (MarseyVars.PatchLogAllowed)
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

        marseyLoggerType.GetField("logDelegate", BindingFlags.Public | BindingFlags.Static)!.SetValue(null, logDelegate);
    }

    /// <summary>
    /// Checks loader environment variables, sets relevant flags in MarseyVars
    ///
    /// Executed only by the loader.
    /// </summary>
    public static void SetupLogFlags()
    {
        MarseyVars.PatchLogAllowed = CheckEnv("MARSEY_LOG_PATCHES");
        MarseyVars.DebugAllowed = CheckEnv("MARSEY_LOADER_DEBUG");
        MarseyVars.ThrowOnFail = CheckEnv("MARSEY_THROW_FAIL");
    }

    private static bool CheckEnv(string envName)
    {
        var envVar = Environment.GetEnvironmentVariable(envName)!;
        return !string.IsNullOrEmpty(envVar) && bool.Parse(envVar);
    }
}
