using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Marsey.Serializer;

public static class Marserializer
{
    public static void Serialize(string[]? path, string filename, List<string> patches)
    {
        var jsonString = JsonSerializer.Serialize(patches);
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, filename);
        File.WriteAllText(fullPath, jsonString);
    }

    public static List<string>? Deserialize(string[]? path, string filename)
    {
        string fullPath = Path.Combine(path ?? Array.Empty<string>());
        fullPath = Path.Combine(fullPath, filename);
        if (File.Exists(fullPath))
        {
            string? jsonString = File.ReadAllText(fullPath);
            List<string>? patches = JsonSerializer.Deserialize<List<string>>(jsonString);
            return patches;
        }
        return null;
    }
}