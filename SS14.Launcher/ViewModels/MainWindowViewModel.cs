using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DynamicData;
using HarmonyLib;
using Marsey;
using Marsey.API;
using Marsey.Config;
using Marsey.Game.Managers;
using Marsey.Stealthsey;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Marseyverse;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.Login;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IErrorOverlayOwner
{
    private readonly DataManager _cfg;
    private readonly LoginManager _loginMgr;
    private readonly HttpClient _http;
    private readonly LauncherInfoManager _infoManager;

    private int _selectedIndex;

    public DataManager Cfg => _cfg;
    [Reactive] public bool OutOfDate { get; private set; }

    public HomePageViewModel HomeTab { get; }
    public ServerListTabViewModel ServersTab { get; }
    public NewsTabViewModel NewsTab { get; }

    public PatchesTabViewModel PatchesTab { get; }
    public OptionsTabViewModel OptionsTab { get; }

    public MainWindowViewModel()
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        _http = Locator.Current.GetRequiredService<HttpClient>();
        _infoManager = Locator.Current.GetRequiredService<LauncherInfoManager>();

        HarmonyManager.Init(new Harmony(MarseyVars.Identifier));
        Hidesey.Initialize();

        ServersTab = new ServerListTabViewModel(this);
        NewsTab = new NewsTabViewModel();
        HomeTab = new HomePageViewModel(this);
        PatchesTab = new PatchesTabViewModel();
        OptionsTab = new OptionsTabViewModel();

        var tabs = new List<MainWindowTabViewModel>();
        tabs.Add(HomeTab);
        tabs.Add(ServersTab);
        tabs.Add(NewsTab);
        tabs.Add(PatchesTab);
        tabs.Add(OptionsTab);
#if DEVELOPMENT
        tabs.Add(new DevelopmentTabViewModel());
#endif
        Tabs = tabs;

        AccountDropDown = new AccountDropDownViewModel(this);
        LoginViewModel = new MainWindowLoginViewModel();

        if (_cfg.GetCVar(CVars.NoActiveInit))
            _loginMgr.ActiveAccount = null;

        this.WhenAnyValue(x => x._loginMgr.ActiveAccount)
            .Subscribe(s =>
            {
                this.RaisePropertyChanged(nameof(Username));
                this.RaisePropertyChanged(nameof(LoggedIn));
                this.RaisePropertyChanged(nameof(LoginText));
                this.RaisePropertyChanged(nameof(ManageAccountText));
            });

        _cfg.Logins.Connect()
            .Subscribe(_ => { this.RaisePropertyChanged(nameof(AccountDropDownVisible)); });

        // If we leave the login view model (by an account getting selected)
        // we reset it to login state
        this.WhenAnyValue(x => x.LoggedIn)
            .DistinctUntilChanged() // Only when change.
            .Subscribe(x =>
            {
                if (x)
                {
                    // "Switch" to main window.
                    RunSelectedOnTab();
                }
                else
                {
                    LoginViewModel.SwitchToLogin();
                }
            });
    }

    public MainWindow? Control { get; set; }

    public IReadOnlyList<MainWindowTabViewModel> Tabs { get; }

    public bool LoggedIn => _loginMgr.ActiveAccount != null;
    public string LoginText => LoggedIn ? $"'Logged in' as {Username}." : "Not logged in.";
    public string ManageAccountText => LoggedIn ? "Change Account..." : "Log in...";
    private string? Username => _loginMgr.ActiveAccount?.Username;
    public bool AccountDropDownVisible => _loginMgr.Logins.Count != 0;

    public AccountDropDownViewModel AccountDropDown { get; }

    public MainWindowLoginViewModel LoginViewModel { get; }

    [Reactive] public ConnectingViewModel? ConnectingVM { get; set; }

    [Reactive] public string? BusyTask { get; private set; }
    [Reactive] public ViewModelBase? OverlayViewModel { get; private set; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var previous = Tabs[_selectedIndex];
            previous.IsSelected = false;

            this.RaiseAndSetIfChanged(ref _selectedIndex, value);

            RunSelectedOnTab();
        }
    }

    private void RunSelectedOnTab()
    {
        var tab = Tabs[_selectedIndex];
        tab.IsSelected = true;
        tab.Selected();
    }

    public ICVarEntry<bool> HasDismissedEarlyAccessWarning => Cfg.GetCVarEntry(CVars.HasDismissedEarlyAccessWarning);

    public async void OnWindowInitialized()
    {
        BusyTask = "Checking MarseyApi";
        await MarseyApi.Initialize(_cfg.GetCVar(CVars.MarseyApiEndpoint), _cfg.GetCVar(CVars.MarseyApi));
        BusyTask = "Checking for launcher update...";
        CheckLauncherUpdate();
        BusyTask = "Refreshing login status...";
        await CheckAccounts();
        BusyTask = null;

        if (_cfg.SelectedLoginId is { } g && _loginMgr.Logins.TryLookup(g, out var login))
        {
            TrySwitchToAccount(login);
        }

        // We should now start reacting to commands.
    }

    private async Task CheckAccounts()
    {
        // Check if accounts are still valid and refresh their tokens if necessary.
        await _loginMgr.Initialize();
    }

    public void OnDiscordButtonPressed()
    {
        Helpers.OpenUri(new Uri(ConfigConstants.DiscordUrl));
    }

    public void OnWebsiteButtonPressed()
    {
        Helpers.OpenUri(new Uri(ConfigConstants.WebsiteUrl));
    }

    private void CheckLauncherUpdate()
    {
        // await Task.Delay(1000);
        if (!ConfigConstants.DoVersionCheck)
        {
            return;
        }
        try
        {
            Version? minimum = MarseyApi.GetMinimumVersion();

            if (minimum == null)
            {
                OutOfDate = false;
                return;
            }

            int comp = MarseyVars.MarseyVersion.CompareTo(minimum);
            if (comp < 0 && !_cfg.GetCVar(CVars.MarseyApiIgnoreForced)) OutOfDate = true;
        }
        catch (HttpRequestException e)
        {
            Log.Warning(e, "Unable to check for launcher update due to error, assuming up-to-date.");
            OutOfDate = false;
        }
    }

    public void ExitPressed()
    {
        Control?.Close();
    }

    public void DownloadPressed()
    {
        string? url = MarseyApi.GetReleasesURL();
        if (string.IsNullOrEmpty(url))
            url = ConfigConstants.DownloadUrl;

        Helpers.OpenUri(new Uri(url));
    }

    public void DismissEarlyAccessPressed()
    {
        Cfg.SetCVar(CVars.HasDismissedEarlyAccessWarning, true);
        Cfg.CommitConfig();
    }

    public void SelectTabServers()
    {
        SelectedIndex = Tabs.IndexOf(ServersTab);
    }

    public void TrySwitchToAccount(LoggedInAccount account)
    {
        switch (account.Status)
        {
            case AccountLoginStatus.Unsure:
                TrySelectUnsureAccount(account);
                break;

            case AccountLoginStatus.Available:
                _loginMgr.ActiveAccount = account;
                break;

            case AccountLoginStatus.Expired:
                _loginMgr.ActiveAccount = null;
                LoginViewModel.SwitchToExpiredLogin(account);
                break;

            case AccountLoginStatus.Guest:
                _loginMgr.ActiveAccount = account;
                break;
        }
    }

    private async void TrySelectUnsureAccount(LoggedInAccount account)
    {
        if (account.Status == AccountLoginStatus.Guest)
        {
            _loginMgr.ActiveAccount = account;
            return;
        }

        BusyTask = "Checking account status";
        try
        {
            await _loginMgr.UpdateSingleAccountStatus(account);

            // Can't be unsure, that'd have thrown.
            Debug.Assert(account.Status != AccountLoginStatus.Unsure);
            TrySwitchToAccount(account);
        }
        catch (AuthApiException e)
        {
            Log.Warning(e, "AuthApiException while trying to refresh account {login}", account.LoginInfo);
            OverlayViewModel = new AuthErrorsOverlayViewModel(this, "Error connecting to authentication server",
                new[]
                {
                    e.InnerException?.Message ?? "Unknown error occured"
                });
        }
        finally
        {
            BusyTask = null;
        }
    }

    public void OverlayOk()
    {
        OverlayViewModel = null;
    }

    public bool IsContentBundleDropValid(IStorageFile file)
    {
        // Can only load content bundles if logged in, in some capacity.
        if (!LoggedIn)
            return false;

        // Disallow if currently connecting to a server.
        if (ConnectingVM != null)
            return false;

        return Path.GetExtension(file.Name) == ".zip";
    }

    public void Dropped(IStorageFile file)
    {
        // Trust view validated this.
        Debug.Assert(IsContentBundleDropValid(file));

        ConnectingViewModel.StartContentBundle(this, file);
    }

    private readonly TitleManager _title = new TitleManager();

    public string RandomTitle => _title.RandomTitle;

}
