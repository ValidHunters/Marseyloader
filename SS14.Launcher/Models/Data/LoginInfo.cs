using System;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SS14.Launcher.Models.Data;

[Serializable]
// Without OptIn JSON.NET chokes on ReactiveObject.
[JsonObject(MemberSerialization.OptIn)]
public class LoginInfo : ReactiveObject
{
    [JsonProperty]
    [Reactive]
    public Guid UserId { get; set; }
    [JsonProperty]
    [Reactive]
    public string Username { get; set; } = default!;
    [JsonProperty]
    [Reactive]
    public LoginToken Token { get; set; }

    public override string ToString()
    {
        return $"{Username}/{UserId}";
    }
}