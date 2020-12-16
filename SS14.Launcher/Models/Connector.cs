using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.Models
{
    public class Connector : ReactiveObject
    {
        private readonly Updater _updater;
        private readonly DataManager _cfg;
        private readonly LoginManager _loginManager;
        private readonly IEngineManager _engineManager;

        private ConnectionStatus _status = ConnectionStatus.None;
        private bool _clientExitedBadly;

        public Connector(Updater updater, DataManager cfg, LoginManager loginManager, IEngineManager engineManager)
        {
            _updater = updater;
            _cfg = cfg;
            _loginManager = loginManager;
            _engineManager = engineManager;
        }

        public ConnectionStatus Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public bool ClientExitedBadly
        {
            get => _clientExitedBadly;
            private set => this.RaiseAndSetIfChanged(ref _clientExitedBadly, value);
        }

        public async void Connect(string address)
        {
            try
            {
                await ConnectInternalAsync(address);
            }
            catch (ConnectException e)
            {
                Log.Error(e, "Failed to connect: {status}", e.Status);
                Status = e.Status;
            }
        }

        private async Task ConnectInternalAsync(string address)
        {
            Status = ConnectionStatus.Connecting;

            var (info, parsedAddr, infoAddr) = await GetServerInfoAsync(address);

            // Run update.
            Status = ConnectionStatus.Updating;

            var installation = await RunUpdateAsync(info);

            var connectAddress = GetConnectAddress(info, infoAddr);

            Status = ConnectionStatus.StartingClient;

            var clientProc = ConnectLaunchClient(info, installation, connectAddress, parsedAddr);

            // Wait 300ms, if the client exits with a bad error code before that it's probably fucked.
            var waitClient = clientProc.WaitForExitAsync();
            var waitDelay = Task.Delay(300);

            await Task.WhenAny(waitDelay, waitClient);

            if (!clientProc.HasExited)
            {
                Status = ConnectionStatus.ClientRunning;
                await waitClient;
                return;
            }

            ClientExitedBadly = clientProc.ExitCode != 0;
            Status = ConnectionStatus.ClientExited;
        }

        private Process? ConnectLaunchClient(ServerInfo info, InstalledServerContent installedServerContent,
            Uri connectAddress, Uri parsedAddr)
        {
            var cVars = new List<(string, string)>();

            if (info.AuthInformation.Mode != AuthMode.Disabled && _loginManager.ActiveAccount != null)
            {
                var account = _loginManager.ActiveAccount;

                cVars.Add(("auth.token", account.LoginInfo.Token.Token));
                cVars.Add(("auth.userid", account.LoginInfo.UserId.ToString()));
                cVars.Add(("auth.serverpubkey", info.AuthInformation.PublicKey));
                cVars.Add(("auth.server", ConfigConstants.AuthUrl));
            }

            try
            {
                // Launch client.
                return LaunchClient(info.BuildInformation.EngineVersion, installedServerContent, new[]
                {
                    // We are using the launcher. Don't show main menu etc..
                    "--launcher",

                    // Pass username to launched client.
                    // We don't load username from client_config.toml when launched via launcher.
                    "--username", _loginManager.ActiveAccount?.Username ?? "JoeGenero",

                    // Connection address
                    "--connect-address", connectAddress.ToString(),

                    // ss14(s):// address passed in. Only used for feedback in the client.
                    "--ss14-address", parsedAddr.ToString(),

                    // GLES2 forcing or using default fallback
                    "--cvar", "display.renderer=" + (_cfg.ForceGLES2 ? "3" : "0"),
                }, cVars);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception while starting client");
                return null;
            }
        }

        private static Uri GetConnectAddress(ServerInfo info, Uri infoAddr)
        {
            if (string.IsNullOrEmpty(info.ConnectAddress))
            {
                // No connect address specified, use same address/port as base address.
                return new UriBuilder
                {
                    Scheme = "udp",
                    Host = infoAddr.Host,
                    Port = infoAddr.Port
                }.Uri;
            }

            try
            {
                return new Uri(info.ConnectAddress);
            }
            catch (FormatException e)
            {
                Log.Error(e, "Failed to parse ConnectAddress");
                throw new ConnectException(ConnectionStatus.ConnectionFailed);
            }
        }

        private async Task<InstalledServerContent> RunUpdateAsync(ServerInfo info)
        {
            var installation = await _updater.RunUpdateForLaunchAsync(info.BuildInformation);
            if (installation == null)
            {
                throw new ConnectException(ConnectionStatus.UpdateError);
            }

            return installation;
        }

        private async Task<(ServerInfo, Uri, Uri)> GetServerInfoAsync(string address)
        {
            var parsedAddress = UriHelper.ParseSs14Uri(address);

            // Fetch server connect info.
            var infoAddr = UriHelper.GetServerInfoAddress(parsedAddress);

            try
            {
                var resp = await Global.GlobalHttpClient.GetStringAsync(infoAddr);
                var info = JsonConvert.DeserializeObject<ServerInfo>(resp);
                return (info, parsedAddress, infoAddr);
            }
            catch (Exception e) when (e is JsonException || e is HttpRequestException)
            {
                throw new ConnectException(ConnectionStatus.ConnectionFailed, e);
            }
        }

        private Process? LaunchClient(
            string engineVersion,
            InstalledServerContent installedServerContent,
            IEnumerable<string> extraArgs,
            List<(string, string)> cVars)
        {
            var pubKey = Path.Combine(LauncherPaths.DirLauncherInstall, "signing_key");
            var binPath = _engineManager.GetEnginePath(engineVersion);
            var sig = _engineManager.GetEngineSignature(engineVersion);
            var contentPath = Path.Combine(LauncherPaths.DirServerContent,
                installedServerContent.DiskId.ToString(CultureInfo.InvariantCulture) + ".zip");

            var startInfo = GetLoaderStartInfo();

            startInfo.ArgumentList.Add(binPath);
            startInfo.ArgumentList.Add(sig);
            startInfo.ArgumentList.Add(pubKey);

            startInfo.ArgumentList.Add("--mount-zip");
            startInfo.ArgumentList.Add(contentPath);

            if (cVars.Count != 0)
            {
                var envVarValue = string.Join(';', cVars.Select(p => $"{p.Item1}={p.Item2}"));
                startInfo.EnvironmentVariables["ROBUST_CVARS"] = envVarValue;
            }

            startInfo.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "LatestMajor";

            startInfo.UseShellExecute = false;
            startInfo.ArgumentList.AddRange(extraArgs);
            return Process.Start(startInfo);
        }

        private static ProcessStartInfo GetLoaderStartInfo()
        {
            string basePath;

#if FULL_RELEASE
            const bool release = true;
#else
            const bool release = false;
#endif

            if (release)
            {
                basePath = Path.Combine(LauncherPaths.DirLauncherInstall, "loader");
            }
            else
            {
                basePath = Path.GetFullPath(Path.Combine(
                    LauncherPaths.DirLauncherInstall,
                    "..", "..", "..", "..",
                    "SS14.Loader", "bin", "Debug", "net5.0"));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new ProcessStartInfo
                {
                    FileName = Path.Combine(basePath, "SS14.Loader")
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new ProcessStartInfo
                {
                    FileName = Path.Combine(basePath, "SS14.Loader.exe"),
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (release)
                {
                    return new ProcessStartInfo
                    {
                        FileName = "open",
                        ArgumentList = {Path.Combine(basePath, "Space Station 14.app"), "--args"},
                    };
                }
                else
                {
                    return new ProcessStartInfo
                    {
                        FileName = Path.Combine(basePath, "SS14.Loader"),
                    };
                }
            }

            throw new NotSupportedException("Unsupported platform.");
        }

        public enum ConnectionStatus
        {
            None,
            Updating,
            UpdateError,
            Connecting,
            ConnectionFailed,
            StartingClient,
            ClientRunning,
            ClientExited
        }

        private sealed class ConnectException : Exception
        {
            public ConnectionStatus Status { get; }

            public ConnectException(ConnectionStatus status)
            {
                Status = status;
            }

            public ConnectException(ConnectionStatus status, Exception inner)
                : base($"Failed to connect: {status}", inner)
            {
                Status = status;
            }
        }
    }
}
