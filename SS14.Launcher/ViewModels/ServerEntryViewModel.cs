using System;
using Avalonia.Media;
using ReactiveUI;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.ViewModels
{
    public class ServerEntryViewModel : ViewModelBase
    {
        private int _ping;
        private bool _isAltBackground;

        public ServerEntryViewModel(string name, int ping)
        {
            Name = name;
            Ping = ping;

            this.WhenAnyValue(x => x.IsAltBackground)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(BackgroundColor)));
        }

        public string Name { get; }
        public string Description { get; } = "Get honked on!";
        public string PlayerCountString => $"{PlayerCount} players";
        public int PlayerCount { get; }

        // Avalonia can't currently do alternating backgrounds in ItemsControl easily.
        // So we have to implement them manually in the view model.
        public bool IsAltBackground
        {
            get => _isAltBackground;
            set => this.RaiseAndSetIfChanged(ref _isAltBackground, value);
        }

        public Color BackgroundColor => IsAltBackground ? Color.Parse("#262626") : Colors.Transparent;

        public int Ping
        {
            get => _ping;
            set => this.RaiseAndSetIfChanged(ref _ping, value);
        }
    }
}