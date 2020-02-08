using System.Diagnostics;
using System.IO;

namespace SS14.Launcher.Bootstrap
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var path = typeof(Program).Assembly.Location;
            var ourDir = Path.GetDirectoryName(path);
            Debug.Assert(ourDir != null);

            var dotnetDir = Path.Combine(ourDir, "dotnet");
            var exeDir = Path.Combine(ourDir, "bin", "SS14.Launcher.exe");

            Process.Start(new ProcessStartInfo(exeDir)
            {
                EnvironmentVariables = {{"DOTNET_ROOT", dotnetDir}}
            });
        }
    }
}