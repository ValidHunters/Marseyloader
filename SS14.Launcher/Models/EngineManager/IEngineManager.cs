using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SS14.Launcher.Models.EngineManager;
// This is an interface instead of a class because
// I was originally planning to make Steam builds bundle the engine with the Steam download.

/// <summary>
///     Manages engine installations.
/// </summary>
public interface IEngineManager
{
    string GetEnginePath(string engineVersion);
    string GetEngineSignature(string engineVersion);

    Task<EngineModuleManifest> GetEngineModuleManifest(CancellationToken cancel = default);

    /// <returns>True if something new had to be installed.</returns>
    Task<bool> DownloadEngineIfNecessary(
        string engineVersion,
        Helpers.DownloadProgressCallback? progress = null,
        CancellationToken cancel = default);

    /// <returns>True if something new had to be installed.</returns>
    Task<bool> DownloadModuleIfNecessary(
        string moduleName,
        string engineVersion,
        EngineModuleManifest manifest,
        Helpers.DownloadProgressCallback? progress = null,
        CancellationToken cancel = default);

    Task DoEngineCullMaybeAsync();
    void ClearAllEngines();
    string GetEngineModule(string moduleName, string moduleVersion);
}

public sealed record EngineModuleManifest(
    Dictionary<string, EngineModuleData> Modules
);

public sealed record EngineModuleData(
    Dictionary<string, EngineModuleVersionData> Versions
);

public sealed record EngineModuleVersionData(
    Dictionary<string, EngineModulePlatformData> Platforms
);

public sealed record EngineModulePlatformData(
    string Url,
    string Sha256,
    string Sig
);
