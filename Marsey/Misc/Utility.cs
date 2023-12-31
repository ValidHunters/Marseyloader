using System;
using System.Reflection;
using Marsey.Config;
using Marsey.PatchAssembly;
using Marsey.Stealthsey;

namespace Marsey.Misc;

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
    public static void Log(LogType logType, string message)
    {
        if (logType == LogType.DEBG && MarseyConf.DebugAllowed != true)
            return;

        SharedLog($"[{logType.ToString()}] {message}");
    }
    
    /// <summary>
    /// Ditto but specifying a system used
    /// </summary>
    public static void Log(LogType logType, string system, string message)
    {
        if (logType == LogType.DEBG && MarseyConf.DebugAllowed != true)
            return;

        SharedLog($"[{logType.ToString()}] [{system}] {message}");
    }

    /// <summary>
    /// Log function used by patches
    /// </summary>
    /// <param name="asm">Assembly name of patch</param>
    /// <param name="message">Log message</param>
    /// <see cref="AssemblyFieldHandler.SetupLogger"/>
    public static void Log(AssemblyName asm, string message)
    {
        SharedLog($"[{asm.Name}] {message}");
    }

    private static void SharedLog(string message)
    {
        if (MarseyConf.Logging)
            Console.WriteLine($"[{MarseyVars.MarseyLoggerPrefix}] {message}");
    }
}
public abstract class Utility
{
    /// <summary>
    /// Checks loader environment variables, sets relevant flags in MarseyVars.
    /// Executed only by the loader.
    /// </summary>
    public static void SetupFlags()
    {
        MarseyConf.Logging = CheckEnv("MARSEY_LOGGING");
        MarseyConf.DebugAllowed = CheckEnv("MARSEY_LOADER_DEBUG");
        MarseyConf.ThrowOnFail = CheckEnv("MARSEY_THROW_FAIL");
        MarseyConf.SeparateLogger = CheckEnv("MARSEY_SEPARATE_LOGGER");
        MarseyConf.ForceHWID = CheckEnv("MARSEY_FORCINGHWID");
        MarseyConf.KillRPC = CheckEnv("MARSEY_DISABLE_PRESENCE");
    }

    private static bool CheckEnv(string envName)
    {
        string envVar = Environment.GetEnvironmentVariable(envName)!;
        Envsey.CleanFlag(envName);
        return !string.IsNullOrEmpty(envVar) && bool.Parse(envVar);
    }
}
