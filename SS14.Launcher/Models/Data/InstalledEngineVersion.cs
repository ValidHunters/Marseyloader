using Newtonsoft.Json;

namespace SS14.Launcher.Models.Data;

public sealed record InstalledEngineVersion
{
    public InstalledEngineVersion(string version, string signature)
    {
        Version = version;
        Signature = signature;
    }

    [field: JsonProperty(PropertyName = "version")]
    public string Version { get; }

    [field: JsonProperty(PropertyName = "signature")]
    public string Signature { get; }
}