using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Marsey.Marserializer;

public static class Marserializer
{
    private const string Filename = "Marserializer.json";
    
    public static void Serialize(string[]? path, List<string> patches)
    {
        var jsonString = JsonSerializer.Serialize(patches);
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, Filename);
        File.WriteAllText(fullPath, jsonString);
    }

    public static List<string>? Deserialize(string[]? path)
    {
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, Filename);
        if (File.Exists(fullPath))
        {
            string? jsonString = File.ReadAllText(fullPath);
            List<string>? patches = JsonSerializer.Deserialize<List<string>>(jsonString);
            return patches;
        }
        return null;
    }
}