using System.Text.Json.Serialization;

namespace SS14.Launcher.Api;

public static class ServerApi
{
    public sealed record ServerStatus(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("players")]
        int PlayerCount,
        [property: JsonPropertyName("soft_max_players")]
        int SoftMaxPlayerCount);
}
