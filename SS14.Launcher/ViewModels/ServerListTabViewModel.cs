using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;

namespace SS14.Launcher.ViewModels
{
    public class ServerListTabViewModel : MainWindowTabViewModel
    {
        private string _searchString;

        public ServerListTabViewModel()
        {
            var den = new ServerEntryViewModel("Wizard's den", 15);

            AllServers = new ObservableCollection<ServerEntryViewModel>(new List<ServerEntryViewModel>
            {
                den,
                new ServerEntryViewModel("Russian Wizard's den", 30),
                new ServerEntryViewModel("Russian Wizard's den", 30) { IsAltBackground = true},
                new ServerEntryViewModel("Russian Wizard's den", 30),
                new ServerEntryViewModel("Russian Wizard's den", 30) { IsAltBackground = true},
                new ServerEntryViewModel("Russian Wizard's den", 30),
                new ServerEntryViewModel("Russian Wizard's den", 30) { IsAltBackground = true},
            });

            this.WhenAnyValue(x => x.SearchString)
                .Subscribe(v =>
                {
                    SearchedServers.Clear();
                    if (string.IsNullOrEmpty(v))
                    {
                        SearchedServers.AddRange(AllServers);
                    }
                    else
                    {
                        SearchedServers.AddRange(AllServers.Where(s =>
                            s.Name.Contains(v, StringComparison.CurrentCultureIgnoreCase)));
                    }
                });
        }

        public ClientUpdaterViewModel Updater { get; } = new ClientUpdaterViewModel();

        public override string Name => "Servers";

        public string SearchString
        {
            get => _searchString;
            set => this.RaiseAndSetIfChanged(ref _searchString, value);
        }

        public ObservableCollection<ServerEntryViewModel> SearchedServers { get; }
            = new ObservableCollection<ServerEntryViewModel>();

        public ObservableCollection<ServerEntryViewModel> AllServers { get; }
    }
}