using Marsey.Misc;
using Newtonsoft.Json;

namespace Marsey.Game.Resources;

/// <summary>
/// Metadata class for Resource Packs
/// </summary>
public class ResourcePack(string dir)
{
    public string Dir { get; } = dir;
    public string? Name { get; private set; }
    public string? Desc { get; private set; }
    public string? Target { get; private set; } // Specify fork for which this is used
    public bool Enabled { get; set; }

    public void ParseMeta()
    {
        string metaPath = Path.Combine(Dir, "meta.json");
        if (!File.Exists(metaPath))
            throw new RPackException($"Found folder {Dir}, but it didn't have a meta.json");
        
        string jsonData = File.ReadAllText(metaPath);
        dynamic? meta = JsonConvert.DeserializeObject(jsonData);
        
        if (meta == null || meta?.Name == null || meta?.Description == null || meta?.Target == null)
            throw new RPackException("Meta.json is incorrectly formatted.");
        
        Name = meta?.Name ?? string.Empty;
        Desc = meta?.Description ?? string.Empty;
        Target = meta?.Target ?? string.Empty;
    }
}