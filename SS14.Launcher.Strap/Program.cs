using System;
using System.Diagnostics;
using System.IO;

namespace SS14.Launcher.Strap;

public static class Program
{
    public static void Main(string[] args)
    {
        string ourDir = GetAssemblyDirectory();
        string dotnetDir = Path.Combine(ourDir, "dotnet");
        string exeDir = Path.Combine(ourDir, "bin", "SS14.Launcher.exe");

        SetEnvironmentVariable("DOTNET_ROOT", dotnetDir);
        StartProcess(exeDir);
    }

    private static string GetAssemblyDirectory()
    {
        string path = typeof(Program).Assembly.Location;
        return Path.GetDirectoryName(path);
    }

    private static void SetEnvironmentVariable(string variable, string value)
    {
        Environment.SetEnvironmentVariable(variable, value);
    }

    private static void StartProcess(string filePath)
    {
        Process.Start(new ProcessStartInfo(filePath));
    }
}
