using System;
using System.Reflection;
using System.Threading;
using Serilog;

namespace SS14.Launcher.Marsey;

public class MarseyPatcher
{
    private static Assembly RobustAss;
    private static Assembly ClientAss;

    private static void ThreadProc()
    {
        bool loaded = false;
        Log.Debug("Loaded into threadproc");

        while (true)
        {
            Thread.Sleep(2500);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var e in assemblies)
            {
                Log.Debug(e.FullName);
                if (e.FullName.Contains("Robust.Client,"))
                {
                    RobustAss = e;
                    loaded = true;
                }

                if (e.FullName.Contains("Content.Client,") && loaded == true)
                {
                    ClientAss = e;
                    Thread t = new Thread(new ThreadStart(ThreadProc2));
                    t.Start();
                    return;
                }
            }
        }
    }

    private static void ThreadProc2()
    {
        Log.Information("Got both Robust and Content assemblies.");
    }

    public static void Boot()
    {
        Thread t = new Thread(new ThreadStart(ThreadProc));
        t.Start();
    }
}
