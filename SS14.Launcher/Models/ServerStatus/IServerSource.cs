namespace SS14.Launcher.Models.ServerStatus;

/// <summary>
/// Where we get server status and info data from.
/// </summary>
public interface IServerSource
{
    public void UpdateInfoFor(ServerStatusData statusData);
}
