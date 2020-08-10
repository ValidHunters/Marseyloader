using System;
using System.Threading;
using ReactiveUI;

namespace SS14.Launcher.Models
{
    public partial class ServerStatusCache
    {
        private sealed class Data : ReactiveObject, IServerStatusData
        {
            private string? _name;
            private TimeSpan? _ping;
            private int _playerCount;
            private ServerStatusCode _status = ServerStatusCode.FetchingStatus;

            public Data(string address)
            {
                Address = address;
            }

            public string Address { get; }

            public string? Name
            {
                get => _name;
                set => this.RaiseAndSetIfChanged(ref _name, value);
            }

            // BUG: This ping stat is completely wrong currently.
            // See the assignment in ServerStatusCache.cs for why.
            public TimeSpan? Ping
            {
                get => _ping;
                set => this.RaiseAndSetIfChanged(ref _ping, value);
            }

            public ServerStatusCode Status
            {
                get => _status;
                set => this.RaiseAndSetIfChanged(ref _status, value);
            }

            public int PlayerCount
            {
                get => _playerCount;
                set => this.RaiseAndSetIfChanged(ref _playerCount, value);
            }

            public bool DidInitialStatusUpdate { get; set; }

            public CancellationTokenSource? Cancellation { get; set; }
            public SemaphoreSlim StatusSemaphore { get; } = new SemaphoreSlim(1);
        }
    }
}
