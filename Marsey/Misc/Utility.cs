using System;
using System.Reflection;
using Marsey.Config;
using Marsey.IPC;
using Marsey.PatchAssembly;
using Marsey.Stealthsey;

namespace Marsey.Misc;

public static class MarseyLogger
{
    public enum LogType
    {
        INFO,
        WARN,
        ERRO,
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
    public static bool CheckEnv(string envName)
    {
        string envVar = Envsey.CleanFlag(envName)!;
        return !string.IsNullOrEmpty(envVar) && bool.Parse(envVar);
    }

    public static void ReadConf()
    {
        IPC.Client MarseyConfPipeClient = new();
        string config = MarseyConfPipeClient.ConnRecv("MarseyConf");

        Dictionary<string, string> envVars = config.Split(';')
            .Select(kv => kv.Split('='))
            .ToDictionary(kv => kv[0], kv => kv[1]);

        // Apply the environment variables to MarseyConf
        foreach (KeyValuePair<string, string> kv in envVars)
        {
            if (!MarseyConf.EnvVarMap.TryGetValue(kv.Key, value: out Action<string>? value)) continue;

            MarseyLogger.Log(MarseyLogger.LogType.DEBG, $"{kv.Key} read {kv.Value}");
            value(kv.Value);
        }
    }
}
