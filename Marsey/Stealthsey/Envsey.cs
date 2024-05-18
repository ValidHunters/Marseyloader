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
    /// Gets the value of an env variable and then sets it to null
    /// </summary>
    /// <param name="envName">Name of env var</param>
    public static string? CleanFlag(string envName)
    {
        string? temp = Environment.GetEnvironmentVariable(envName);
        Environment.SetEnvironmentVariable(envName, null);
        return temp;
    }
}
