using System;
using Avalonia.Controls;
using ReactiveUI;
using SS14.Launcher.Models;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels
{
    public class ConnectingViewModel : ViewModelBase
    {
        private readonly Connector _connector;
        private readonly Updater _updater;

        public ConnectingViewModel(Connector connector, Updater updater)
        {
            _connector = connector;
            _updater = updater;

            this.WhenAnyValue(x => x._updater.Progress)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(Progress));
                    this.RaisePropertyChanged(nameof(ProgressIndeterminate));
                    this.RaisePropertyChanged(nameof(ProgressText));
                });

            this.WhenAnyValue(x => x._updater.Status)
                .Subscribe(_ => { this.RaisePropertyChanged(nameof(StatusText)); });

            this.WhenAnyValue(x => x._connector.Status)
                .Subscribe(val =>
                {
                    this.RaisePropertyChanged(nameof(ProgressIndeterminate));
                    this.RaisePropertyChanged(nameof(StatusText));
                    this.RaisePropertyChanged(nameof(ProgressBarVisible));

                    if (val == Connector.ConnectionStatus.ClientRunning
                        || val == Connector.ConnectionStatus.ClientExited && !_connector.ClientExitedBadly)
                    {
                        Window!.Close();
                    }
                });

            this.WhenAnyValue(x => x._connector.ClientExitedBadly)
                .Subscribe(_ => this.RaisePropertyChanged(StatusText));
        }

        private Window? Window { get; set; }

        public float Progress
        {
            get
            {
                if (_updater.Progress == null)
                {
                    return 0;
                }

                var (downloaded, total) = _updater.Progress.Value;

                return downloaded / (float) total;
            }
        }

        public string ProgressText
        {
            get
            {
                if (_updater.Progress == null)
                {
                    return "";
                }

                var (downloaded, total) = _updater.Progress.Value;

                return $"{Helpers.FormatBytes(downloaded)} / {Helpers.FormatBytes(total)}";
            }
        }

        public bool ProgressIndeterminate
        {
            get
            {
                if (_connector.Status == Connector.ConnectionStatus.Updating)
                {
                    return !_updater.Progress.HasValue;
                }

                return true;
            }
        }

        public bool ProgressBarVisible => _connector.Status != Connector.ConnectionStatus.ClientExited &&
                                          _connector.Status != Connector.ConnectionStatus.ClientRunning &&
                                          _connector.Status != Connector.ConnectionStatus.ConnectionFailed &&
                                          _connector.Status != Connector.ConnectionStatus.UpdateError;

        public string StatusText =>
            _connector.Status switch
            {
                Connector.ConnectionStatus.None => "Starting connection...",
                Connector.ConnectionStatus.UpdateError =>
                "There was an error while updating the client. Please ensure you can access builds.spacestation14.io and try again. Or ask on Discord since it's probably our fault anyways.",
                Connector.ConnectionStatus.Updating => "Updating: " + _updater.Status switch
                {
                    Updater.UpdateStatus.CheckingClientUpdate => "Checking for client update...",
                    Updater.UpdateStatus.DownloadingClientUpdate => "Downloading client update...",
                    Updater.UpdateStatus.Extracting => "Extracting update...",
                    Updater.UpdateStatus.Verifying => "Verifying download integrity...",
                    Updater.UpdateStatus.Ready => "Update done!",
                    _ => "You shouldn't see this"
                },
                Connector.ConnectionStatus.Connecting => "Fetching connection info from server...",
                Connector.ConnectionStatus.ConnectionFailed => "Failed to connect to server!",
                Connector.ConnectionStatus.StartingClient => "Starting client...",
                Connector.ConnectionStatus.ClientExited => _connector.ClientExitedBadly
                    ? "Client seems to have crashed while starting. If this persists, please ask on Discord or GitHub for support."
                    : "",
                _ => ""
            };

        public static void StartConnect(Updater updater, DataManager cfg, Window window, string address)
        {
            var connector = new Connector(updater, cfg);
            var connectionViewModel = new ConnectingViewModel(connector, updater);
            var dialog = new ConnectingDialog {DataContext = connectionViewModel};
            connectionViewModel.Window = dialog;
            connectionViewModel.Start(address);
            dialog.ShowDialog(window);
        }

        private void Start(string address)
        {
            _connector.Connect(address);
        }
    }
}
