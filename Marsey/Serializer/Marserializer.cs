using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Marsey.Serializer;

/// <summary>
/// Handles patchlist serialization
/// </summary>
public static class Marserializer
{
    /// <inheritdoc cref="Marserializer"/>
    public static void Serialize(string[]? path, string filename, List<string> patches)
    {
        string jsonString = JsonSerializer.Serialize(patches);
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, filename);
        File.WriteAllText(fullPath, jsonString);
    }

    /// <inheritdoc cref="Marserializer"/>
    public static List<string>? Deserialize(string[]? path, string filename)
    {
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, filename);
        
        if (!File.Exists(fullPath)) return null;
        
        string jsonString = File.ReadAllText(fullPath);
        List<string>? patches = JsonSerializer.Deserialize<List<string>>(jsonString);
        return patches;
    }
}