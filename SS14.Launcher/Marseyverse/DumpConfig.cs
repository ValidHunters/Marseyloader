using System;
using System.Collections.Generic;
using System.Reflection;
using Marsey.Config;
using Marsey.Misc;
using Serilog;

namespace SS14.Launcher.Marseyverse;

public static class DumpConfig
{
    public static void Dump()
    {
        Dictionary<string, object?> VarDict = new Dictionary<string, object?>();
        Type vars = typeof(MarseyVars);

        foreach (FieldInfo field in vars.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            VarDict.Add(field.Name, field.GetValue(null));
        }

        foreach ((string? key, object? value) in VarDict)
        {
            Log.Information($"{key}: {value}");
        }


    }
}
