using System;
using System.Reflection;

namespace Marsey;

public static class MarseyLogger
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
    public static void Log(MarseyLogger.LogType logType, string message)
    {
        if (logType == MarseyLogger.LogType.DEBG && MarseyVars.DebugAllowed != true)
            return;

        Console.WriteLine($"[MARSEY] [{logType.ToString()}] {message}");
    }
    
    /// <summary>
    /// Log function used by patches
    /// </summary>
    /// <param name="asm">Assembly name of patch</param>
    /// <param name="message">Log message</param>
    /// <see cref="AssemblyFieldHandler.SetupLogger"/>
    public static void Log(AssemblyName asm, string message)
    {
        if (MarseyVars.PatchLogAllowed)
            Console.WriteLine($"[{asm.Name}] {message}");
    }
}
public abstract class Utility
{
    /// <summary>
    /// Checks loader environment variables, sets relevant flags in MarseyVars
    ///
    /// Executed only by the loader.
    /// </summary>
    public static void SetupFlags()
    {
        MarseyVars.PatchLogAllowed = CheckEnv("MARSEY_LOG_PATCHES");
        MarseyVars.DebugAllowed = CheckEnv("MARSEY_LOADER_DEBUG");
        MarseyVars.ThrowOnFail = CheckEnv("MARSEY_THROW_FAIL");
        MarseyVars.Subverter = CheckEnv("MARSEY_SUBVERTER");
    }

    private static bool CheckEnv(string envName)
    {
        var envVar = Environment.GetEnvironmentVariable(envName)!;
        return !string.IsNullOrEmpty(envVar) && bool.Parse(envVar);
    }
}
