using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using Splat;

namespace SS14.Launcher.Models.ServerStatus
{
    /// <summary>
    ///     Caches information pulled from servers and updates it asynchronously.
    ///     So we don't have to request data from favorite servers twice (once server list, once favorites list).
    /// </summary>
    public partial class ServerStatusCache
    {
        // Yes this class "memory leaks" because it never frees these data objects.
        // Oh well!
        private readonly Dictionary<string, Data> _cachedData
            = new Dictionary<string, Data>();

        private readonly HttpClient _http;

        public ServerStatusCache()
        {
            _http = Locator.Current.GetService<HttpClient>();
        }

        /// <summary>
        ///     Gets an uninitialized status for a server address.
        ///     This does NOT start fetching the data.
        /// </summary>
        /// <param name="serverAddress">The address of the server to fetch data for.</param>
        public IServerStatusData GetStatusFor(string serverAddress)
        {
            if (_cachedData.TryGetValue(serverAddress, out var data))
            {
                return data;
            }

            data = new Data(serverAddress);
            _cachedData.Add(serverAddress, data);

            return data;
        }

        /// <summary>
        ///     Do the initial status update for a server status. This only acts once.
        /// </summary>
        public void InitialUpdateStatus(IServerStatusData data)
        {
            var actualData = (Data) data;
            if (actualData.DidInitialStatusUpdate)
            {
                return;
            }

            actualData.DidInitialStatusUpdate = true;
            UpdateStatusFor(actualData);
        }

        private async void UpdateStatusFor(Data data)
        {
            await data.StatusSemaphore.WaitAsync();
            var cancelSource = data.Cancellation = new CancellationTokenSource();
            var cancel = cancelSource.Token;

            try
            {
                if (!UriHelper.TryParseSs14Uri(data.Address, out var parsedAddress))
                {
                    Log.Warning("Server {Server} has invalid URI {Uri}", data.Name, data.Address);
                    data.Status = ServerStatusCode.Offline;
                    return;
                }

                var statusAddr = UriHelper.GetServerStatusAddress(parsedAddress);
                data.Status = ServerStatusCode.FetchingStatus;

                var stopwatch = new Stopwatch();
                ServerStatus status;
                try
                {
                    // BUG: This ping stat is completely wrong currently.
                    // TCP/TLS need multiple round trips, which we are measuring.
                    stopwatch.Start();
                    using var response = await _http.GetAsync(statusAddr, cancel);
                    stopwatch.Stop();
                    response.EnsureSuccessStatusCode();
                    status = JsonConvert.DeserializeObject<ServerStatus>(await response.Content.ReadAsStringAsync());

                    if (cancel.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                }
                catch (Exception e) when (e is JsonException || e is HttpRequestException)
                {
                    data.Status = ServerStatusCode.Offline;
                    return;
                }

                data.Status = ServerStatusCode.Online;
                data.Ping = stopwatch.Elapsed;
                data.Name = status.Name;
                data.PlayerCount = status.PlayerCount;
            }
            catch (TaskCanceledException)
            {
                // Do nothing.
            }
            finally
            {
                data.Cancellation = null;
                data.StatusSemaphore.Release();
            }
        }

        public void Refresh()
        {
            // TODO: This refreshes everything.
            // Which means if you're hitting refresh on your home page, it'll refresh the servers list too.
            // This is wasteful.

            foreach (var datum in _cachedData.Values)
            {
                if (!datum.DidInitialStatusUpdate)
                {
                    continue;
                }

                datum.Cancellation?.Cancel();

                UpdateStatusFor(datum);
            }
        }

        private sealed class ServerStatus
        {
            [JsonProperty(PropertyName = "name")] public string? Name { get; set; }

            [JsonProperty(PropertyName = "players")]
            public int PlayerCount { get; set; }
        }
    }
}
