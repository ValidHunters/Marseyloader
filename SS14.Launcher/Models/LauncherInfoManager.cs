using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Serilog;

namespace SS14.Launcher.Models;

/// <summary>
/// Fetches and caches information from <see cref="ConfigConstants.UrlLauncherInfo"/>.
/// </summary>
public sealed class LauncherInfoManager(HttpClient httpClient)
{
    private readonly Random _messageRandom = new();
    private string[]? _messages;

    public void Initialize()
    {
        LoadData();
    }

    private async void LoadData()
    {
        LauncherInfoModel? info;
        try
        {
            Log.Debug("Loading launcher info... {Url}", ConfigConstants.UrlLauncherInfo);
            info = await httpClient.GetFromJsonAsync<LauncherInfoModel>(ConfigConstants.UrlLauncherInfo);
            if (info == null)
            {
                Log.Warning("Launcher info response was null.");
                return;
            }
        }
        catch (HttpRequestException e)
        {
            Log.Warning(e, "Loading launcher info failed");
            return;
        }

        // This is future-proofed to support multiple languages,
        // but for now the launcher only supports English so it'll have to do.
        info.Messages.TryGetValue("en-US", out _messages);
    }

    public string? GetRandomMessage()
    {
        if (_messages == null)
            return null;

        return _messages[_messageRandom.Next(_messages.Length)];
    }

    private sealed record LauncherInfoModel(Dictionary<string, string[]> Messages);
}
