using System;
using Newtonsoft.Json;
using ReactiveUI;

namespace SS14.Launcher.Models.Data;

[Serializable]
// Without OptIn JSON.NET chokes on ReactiveObject.
[JsonObject(MemberSerialization.OptIn)]
public sealed class FavoriteServer : ReactiveObject
{
    private string? _name;

    // For serialization.
    public FavoriteServer()
    {
        Address = default!;
    }

    public FavoriteServer(string? name, string address)
    {
        Name = name;
        Address = address;
    }

    [JsonProperty(PropertyName = "name")]
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    [JsonProperty(PropertyName = "address")]
    public string Address { get; private set; } // Need private set for JSON.NET to work.
}