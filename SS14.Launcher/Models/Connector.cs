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
using SS14.Launcher.Models.Logins;

namespace SS14.Launcher.Models
{
    public class Connector : ReactiveObject
    {
        private readonly Updater _updater;
        private readonly DataManager _cfg;
        private readonly LoginManager _loginManager;

        private ConnectionStatus _status = ConnectionStatus.None;
        private bool _clientExitedBadly;

        public Connector(Updater updater, DataManager cfg, LoginManager loginManager)
        {
            _updater = updater;
            _cfg = cfg;
            _loginManager = loginManager;
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
                return;
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

        private Process ConnectLaunchClient(ServerInfo info, Installation installation, Uri connectAddress, Uri parsedAddr)
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

            // Launch client.
            var proc = LaunchClient(installation, new[]
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
            return proc;
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

        private async Task<Installation> RunUpdateAsync(ServerInfo info)
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

        private static Process LaunchClient(
            Installation installation,
            IEnumerable<string> extraArgs,
            List<(string, string)> cVars)
        {
            var binPath = Path.Combine(UserDataDir.GetUserDataDir(), "installations",
                installation.DiskId.ToString(CultureInfo.InvariantCulture));
            ProcessStartInfo startInfo;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(binPath, "Robust.Client")
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(binPath, "Robust.Client.exe"),
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: does this cause macOS to make a security warning?
                // If it does we'll have to manually launch the contents, which is simple enough.
                startInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    ArgumentList = {"Space Station 14.app", "--args"},
                };
            }
            else
            {
                throw new NotSupportedException("Unsupported platform.");
            }

            if (cVars.Count != 0)
            {
                var envVarValue = string.Join(';', cVars.Select(p => $"{p.Item1}={p.Item2}"));
                startInfo.EnvironmentVariables["ROBUST_CVARS"] = envVarValue;
            }

            startInfo.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "LatestMajor";

            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = binPath;
            startInfo.ArgumentList.AddRange(extraArgs);
            return Process.Start(startInfo);
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
