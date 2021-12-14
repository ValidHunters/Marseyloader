using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class HomePageViewModel : MainWindowTabViewModel
{
    public MainWindowViewModel MainWindowViewModel { get; }
    private readonly List<ServerEntryViewModel> _favorites = new List<ServerEntryViewModel>();
    private readonly DataManager _cfg;
    private readonly ServerStatusCache _statusCache;

    public HomePageViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        _cfg = Locator.Current.GetService<DataManager>();
        _statusCache = Locator.Current.GetService<ServerStatusCache>();

        _cfg.FavoriteServers
            .Connect()
            .Subscribe(_ => UpdateFavoritesList());
    }

    public ObservableCollection<ServerEntryViewModel> Favorites { get; }
        = new ObservableCollection<ServerEntryViewModel>();

    [Reactive] public bool FavoritesEmpty { get; private set; } = true;

    public override string Name => "Home";
    public Control? Control { get; set; }

    public async void DirectConnectPressed()
    {
        if (!TryGetWindow(out var window))
        {
            return;
        }

        var res = await new DirectConnectDialog().ShowDialog<string>(window);
        if (res == null)
        {
            return;
        }

        ConnectingViewModel.StartConnect(MainWindowViewModel, res);
    }

    public async void AddFavoritePressed()
    {
        if (!TryGetWindow(out var window))
        {
            return;
        }

        var (name, address) = await new AddFavoriteDialog().ShowDialog<(string name, string address)>(window);

        try
        {
            _cfg.AddFavoriteServer(new FavoriteServer(name, address));
            _cfg.CommitConfig();
        }
        catch (ArgumentException)
        {
            // Happens if address already a favorite, so ignore.
            // TODO: Give a popup to the user?
        }
    }

    private bool TryGetWindow([MaybeNullWhen(false)] out Window? window)
    {
        window = Control?.GetVisualRoot() as Window;
        return window != null;
    }

    private void UpdateFavoritesList()
    {
        // TODO: This is O(n^2)
        _favorites.RemoveAll(p => !_cfg.FavoriteServers.Items.Contains(p.Favorite));

        // TODO: This is also O(n^2)
        foreach (var favoriteServer in _cfg.FavoriteServers.Items)
        {
            if (_favorites.Any(f => f.Favorite == favoriteServer))
            {
                continue;
            }

            var serverEntryViewModel = new ServerEntryViewModel(MainWindowViewModel, favoriteServer);

            serverEntryViewModel.DoInitialUpdate();
            _favorites.Add(serverEntryViewModel);
        }

        _favorites.Sort((a, b) =>
            string.Compare(a.Favorite!.Name, b.Favorite!.Name, StringComparison.CurrentCulture));

        Favorites.Clear();
        Favorites.AddRange(_favorites);

        FavoritesEmpty = Favorites.Count == 0;
    }

    public void RefreshPressed()
    {
        _statusCache.Refresh();
    }
}
