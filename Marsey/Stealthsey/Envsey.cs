using System;
using Marsey.Config;

namespace Marsey.Stealthsey;

/// <summary>
/// Manages hiding Environment variables
/// </summary>
public static class Envsey
{
    /// <summary>
    /// Sets an env variable to null
    /// </summary>
    /// <param name="envName">Name of env var</param>
    public static void CleanFlag(string envName)
    {
        if (MarseyVars.MarseyHide >= HideLevel.Normal)
            Environment.SetEnvironmentVariable(envName, null);
    }
}