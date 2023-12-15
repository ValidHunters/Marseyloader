using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using Marsey;
using Marsey.API;
using Marsey.Config;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using SS14.Launcher.Api;
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

        ServersTab = new ServerListTabViewModel(this);
        NewsTab = new NewsTabViewModel();
        HomeTab = new HomePageViewModel(this);
        PatchesTab = new PatchesTabViewModel();
        OptionsTab = new OptionsTabViewModel();

        Tabs = new MainWindowTabViewModel[]
        {
            HomeTab,
            ServersTab,
            NewsTab,
            PatchesTab,
            OptionsTab
        };

        AccountDropDown = new AccountDropDownViewModel(this);
        LoginViewModel = new MainWindowLoginViewModel();

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
            if (comp < 0) OutOfDate = true;
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
        }
    }

    private async void TrySelectUnsureAccount(LoggedInAccount account)
    {
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

    public bool IsContentBundleDropValid(string fileName)
    {
        // Can only load content bundles if logged in, in some capacity.
        if (!LoggedIn)
            return false;

        // Disallow if currently connecting to a server.
        if (ConnectingVM != null)
            return false;

        return Path.GetExtension(fileName) == ".zip";
    }

    public void Dropped(string fileName)
    {
        // Trust view validated this.
        Debug.Assert(IsContentBundleDropValid(fileName));

        ConnectingViewModel.StartContentBundle(this, fileName);
    }

    private static readonly Random Random = new Random();

    private static readonly string[] Titles =
    {
        "Space Station 14 Launcher", "Dramalauncher",
        "Marsey", "Moonyware", "Marseyloader",
        "Robustcontrol", "Mirailoader", "Almost BepInEx",
        "Video game launcher", "MIT-certified funny",
        "戏剧装载机", "Oldest anarchy launcher in ss14",
        "ILVerifier", "Space Station 13", "BYOND",
        "Goonstation", "BYONDCONTROL", "Unitystation",
        "Stationeers", "RE:SS2D", "Schizostation 14"
    };

    private static readonly string[] TagLines =
    {
        "Marsey is the cutest cat", "Not a cheat loader",
        "PR self-merging solutions", "RSHOE please come back",
        "As audited on discord.gg/ss14", "Leading disabler of engine signature checks",
        "Sigmund Goldberg died for this", "Sandbox sidestep simulator", "DIY bomb tutorials",
        "incel matchmaking service", "#1 ransomware provider", "God, King & Bussy",
        "The mayocide is coming", "Leading forum for misandrists", "Trans Rights!",
        "Largest long bacon provider", "天安門大屠殺", "Shitcode tester",
        "The code for the client literally calls itself a cheat client",
        "Tumblr client for fujoshis", "Cheap airfare and hotels", "Aliyah consulting",
        "Friday Night Funkin mod manager", "download .apk MOD (Infinite money, health, free admin)",
        "Disaster generator", "if (OperatingSystem.IsWindows())", "Primary cause of binary blob discussions",
        "George Bush did 9/11 and yet I'm the bad guy", "Hybristophiliac support group",
        "Space game but awesome", "Game SUCKS I go bed", "Go be ironic on wizden"
    };

    public string RandomTitle => Titles[Random.Next(Titles.Length)] + ": " + TagLines[Random.Next(TagLines.Length)];
}
