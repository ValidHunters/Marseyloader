using System;
using Marsey.Config;
using Marsey.Stealthsey.Reflection;

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
    [HideLevelRequirement(HideLevel.Normal)]
    public static string? CleanFlag(string envName)
    {
        string? temp = Environment.GetEnvironmentVariable(envName);
        Environment.SetEnvironmentVariable(envName, null);
        return temp;
    }
}