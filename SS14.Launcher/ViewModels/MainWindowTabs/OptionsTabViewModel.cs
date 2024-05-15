using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Data.Converters;
using DynamicData;
using Marsey;
using Marsey.Config;
using Marsey.Game.Patches;
using Marsey.Misc;
using Marsey.Stealthsey;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Splat;
using SS14.Launcher.MarseyFluff;
using SS14.Launcher.Models.ContentManagement;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class OptionsTabViewModel : MainWindowTabViewModel, INotifyPropertyChanged
{
    private DataManager Cfg { get; }
    private readonly LoginManager _loginManager;
    private readonly DataManager _dataManager;
    private readonly IEngineManager _engineManager;
    private readonly ContentManager _contentManager;

    public ICommand SetHWIdCommand { get; }
    public ICommand GenHWIdCommand { get; }
    public ICommand DumpConfigCommand { get; }
    public ICommand SetUsernameCommand { get; }
    public ICommand SetRPCUsernameCommand { get; }
    public ICommand SetGuestUsernameCommand { get; }
    public ICommand SetEndpointCommand { get; }
    public IEnumerable<HideLevel> HideLevels { get; } = Enum.GetValues(typeof(HideLevel)).Cast<HideLevel>();


    public OptionsTabViewModel()
    {
        Cfg = Locator.Current.GetRequiredService<DataManager>();
        _loginManager = Locator.Current.GetRequiredService<LoginManager>();
        _dataManager = Locator.Current.GetRequiredService<DataManager>();
        _engineManager = Locator.Current.GetRequiredService<IEngineManager>();
        _contentManager = Locator.Current.GetRequiredService<ContentManager>();

        SetHWIdCommand = new RelayCommand(OnSetHWIdClick);
        SetRPCUsernameCommand = new RelayCommand(OnSetRPCUsernameClick);
        GenHWIdCommand = new RelayCommand(OnGenHWIdClick);
        SetUsernameCommand = new RelayCommand(OnSetUsernameClick);
        SetGuestUsernameCommand = new RelayCommand(OnSetGuestUsernameClick);
        SetEndpointCommand = new RelayCommand(OnSetEndpointClick);
        DumpConfigCommand = new RelayCommand(DumpConfig.Dump);
    }

#if RELEASE
        public bool HideDebugKnobs => true;
#else
    public bool HideDebugKnobs => false;
#endif

    public override string Name => "Options";

    public bool CompatMode
    {
        get => Cfg.GetCVar(CVars.CompatMode);
        set
        {
            Cfg.SetCVar(CVars.CompatMode, value);
            Cfg.CommitConfig();
        }
    }

    public bool DynamicPgo
    {
        get => Cfg.GetCVar(CVars.DynamicPgo);
        set
        {
            Cfg.SetCVar(CVars.DynamicPgo, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogClient
    {
        get => Cfg.GetCVar(CVars.LogClient);
        set
        {
            Cfg.SetCVar(CVars.LogClient, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogLauncher
    {
        get => Cfg.GetCVar(CVars.LogLauncher);
        set
        {
            Cfg.SetCVar(CVars.LogLauncher, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogLauncherVerbose
    {
        get => Cfg.GetCVar(CVars.LogLauncherVerbose);
        set
        {
            Cfg.SetCVar(CVars.LogLauncherVerbose, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogPatches
    {
        get => Cfg.GetCVar(CVars.LogPatcher);
        set
        {
            Cfg.SetCVar(CVars.LogPatcher, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogLoaderDebug
    {
        get => Cfg.GetCVar(CVars.LogLoaderDebug);
        set
        {
            Cfg.SetCVar(CVars.LogLoaderDebug, value);
            Cfg.CommitConfig();
        }
    }

    public bool ThrowPatchFail
    {
        get => Cfg.GetCVar(CVars.ThrowPatchFail);
        set
        {
            Cfg.SetCVar(CVars.ThrowPatchFail, value);
            Cfg.CommitConfig();
        }
    }

    public bool SeparateLogging
    {
        get => Cfg.GetCVar(CVars.SeparateLogging);
        set
        {
            Cfg.SetCVar(CVars.SeparateLogging, value);
            Cfg.CommitConfig();
        }
    }

    public HideLevel HideLevel
    {
        get => (HideLevel)Cfg.GetCVar(CVars.MarseyHide);
        set
        {
            Cfg.SetCVar(CVars.MarseyHide, (int)value);
            OnPropertyChanged(nameof(HideLevel));
            Cfg.CommitConfig();
        }
    }

    public bool DisableSigning
    {
        get => Cfg.GetCVar(CVars.DisableSigning);
        set
        {
            Cfg.SetCVar(CVars.DisableSigning, value);
            Cfg.CommitConfig();
        }
    }

    public bool MarseyApi
    {
        get => Cfg.GetCVar(CVars.MarseyApi);
        set
        {
            Cfg.SetCVar(CVars.MarseyApi, value);
            OnPropertyChanged(nameof(MarseyApi));
            Cfg.CommitConfig();
        }
    }

    private string _endpoint = "";
    public string MarseyApiEndpoint
    {
        get => Cfg.GetCVar(CVars.MarseyApiEndpoint);
        set => _endpoint = value;
    }

    public bool MarseyApiIgnoreForced
    {
        get => Cfg.GetCVar(CVars.MarseyApiIgnoreForced);
        set
        {
            Cfg.SetCVar(CVars.MarseyApiIgnoreForced, value);
            Cfg.CommitConfig();
        }
    }

    public bool NoActiveInit
    {
        get => Cfg.GetCVar(CVars.NoActiveInit);
        set
        {
            Cfg.SetCVar(CVars.NoActiveInit, value);
            Cfg.CommitConfig();
        }
    }

    public bool DisableRPC
    {
        get => Cfg.GetCVar(CVars.DisableRPC);
        set
        {
            Cfg.SetCVar(CVars.DisableRPC, value);
            Cfg.CommitConfig();
        }
    }

    public bool FakeRPC
    {
        get => Cfg.GetCVar(CVars.FakeRPC);
        set
        {
            Cfg.SetCVar(CVars.FakeRPC, value);
            OnPropertyChanged(nameof(FakeRPC));
            Cfg.CommitConfig();
        }
    }

    private string _RPCUsername = "";
    public string RPCUsername
    {
        get => Cfg.GetCVar(CVars.RPCUsername);
        set => _RPCUsername = value;
    }

    public bool ForcingHWID
    {
        get => Cfg.GetCVar(CVars.ForcingHWId);
        set
        {
            Cfg.SetCVar(CVars.ForcingHWId, value);
            OnPropertyChanged(nameof(ForcingHWID));
            Cfg.CommitConfig();
        }
    }

    public bool LIHWIDBind
    {
        get => Cfg.GetCVar(CVars.LIHWIDBind);
        set
        {
            Cfg.SetCVar(CVars.LIHWIDBind, value);
            Cfg.CommitConfig();
        }
    }

    public bool RandHWID
    {
        get => Cfg.GetCVar(CVars.RandHWID);
        set
        {
            Cfg.SetCVar(CVars.RandHWID, value);
            Cfg.CommitConfig();
        }
    }

    private string _HWIdString = "";
    public string HWIdString
    {
        get
        {
            if (!LIHWIDBind)
                return Cfg.GetCVar(CVars.ForcedHWId);

            return _loginManager.ActiveAccount != null ? _loginManager.ActiveAccount.LoginInfo.HWID : "";
        }
        set => _HWIdString = value;
    }

    private string _GuestUname;
    public string GuestName
    {
        get => Cfg.GetCVar(CVars.GuestUsername);
        set => _GuestUname = value;
    }

    public bool MarseySlightOutOfDate
    {
        get
        {
            if (Latest == null) return false;
            int dist = MarseyVars.MarseyVersion.CompareTo(Marsey.API.MarseyApi.GetLatestVersion());
            return dist < 0;
        }
    }

    public bool MarseyJam
    {
        get => Cfg.GetCVar(CVars.JamDials);
        set
        {
            Cfg.SetCVar(CVars.JamDials, value);
            Cfg.CommitConfig();
        }
    }

    public bool MarseyHole
    {
        get => Cfg.GetCVar(CVars.Blackhole);
        set
        {
            Cfg.SetCVar(CVars.Blackhole, value);
            Cfg.CommitConfig();
        }
    }

    public bool Backports
    {
        get => Cfg.GetCVar(CVars.Backports);
        set
        {
            Cfg.SetCVar(CVars.Backports, value);
            Cfg.CommitConfig();
        }
    }

    public bool DisableAnyEngineBackports
    {
        get => Cfg.GetCVar(CVars.DisableAnyEngineBackports);
        set
        {
            Cfg.SetCVar(CVars.DisableAnyEngineBackports, value);
            Cfg.CommitConfig();
        }
    }

    public bool RandTitle
    {
        get => Cfg.GetCVar(CVars.RandTitle);
        set
        {
            Cfg.SetCVar(CVars.RandTitle, value);
            Cfg.CommitConfig();
        }
    }

    public bool RandHeader
    {
        get => Cfg.GetCVar(CVars.RandHeader);
        set
        {
            Cfg.SetCVar(CVars.RandHeader, value);
            Cfg.CommitConfig();
        }
    }

    public bool RandConnAction
    {
        get => Cfg.GetCVar(CVars.RandConnAction);
        set
        {
            Cfg.SetCVar(CVars.RandConnAction, value);
            Cfg.CommitConfig();
        }
    }

    public string Current => MarseyVars.MarseyVersion.ToString();

    public string? Latest => Marsey.API.MarseyApi.GetLatestVersion()?.ToString();

    public bool OverrideAssets
    {
        get => Cfg.GetCVar(CVars.OverrideAssets);
        set
        {
            Cfg.SetCVar(CVars.OverrideAssets, value);
            Cfg.CommitConfig();
        }
    }

    public string Username
    {
        get => _loginManager.ActiveAccount?.Username!;
        set
        {
            LoginInfo LI = _loginManager.ActiveAccount!.LoginInfo;
            LI.Username = value;
        }
    }

    private void OnSetHWIdClick()
    {
        string hwid = _HWIdString;

        // Check if _HWIdString is a valid hex string (allowing empty string) and pad it if necessary
        if (Regex.IsMatch(_HWIdString, "^[a-fA-F0-9]*$")) // '*' allows for zero or more characters
        {
            if (LIHWIDBind)
            {
                if (_loginManager.ActiveAccount != null)
                {
                    Log.Debug($"Writing {hwid} to {_loginManager.ActiveAccount.Username}");
                    _loginManager.ActiveAccount.LoginInfo.HWID = hwid;
                }
            }
            else
            {
                Log.Debug($"Writing {hwid} to MarseyConf");
                Cfg.SetCVar(CVars.ForcedHWId, _HWIdString);
                Cfg.CommitConfig();
            }

            OnPropertyChanged(nameof(HWIdString));
            return;
        }

        Log.Warning("Passed HWId is not a valid hexadecimal string! Refusing to apply.");
    }

    private void OnSetRPCUsernameClick()
    {
        Cfg.SetCVar(CVars.RPCUsername, _RPCUsername);
        Cfg.CommitConfig();
    }

    private void OnGenHWIdClick()
    {
        string hwid = HWID.GenerateRandom();
        _HWIdString = hwid;
        HWIdString = hwid;

        OnSetHWIdClick();
    }

    public bool DumpAssemblies
    {
        get => MarseyConf.Dumper;
        set => MarseyConf.Dumper = value;
    }

    public bool ResourceOverride
    {
        get => Cfg.GetCVar(CVars.DisableStrict);
        set
        {
            Cfg.SetCVar(CVars.DisableStrict, value);
            Cfg.CommitConfig();
        }
    }

    private void OnSetUsernameClick()
    {
        _dataManager.ChangeLogin(ChangeReason.Update, _loginManager.ActiveAccount?.LoginInfo!);
        _dataManager.CommitConfig();
    }

    private void OnSetGuestUsernameClick()
    {
        Cfg.SetCVar(CVars.GuestUsername, _GuestUname);
        Cfg.CommitConfig();
    }

    private void OnSetEndpointClick()
    {
        if (_endpoint == "") return;

        Task.Run(async () =>
        {
            bool result = await Marsey.API.MarseyApi.MarseyHello(_endpoint).ConfigureAwait(false);
            if (result)
            {
                Cfg.SetCVar(CVars.MarseyApiEndpoint, _endpoint);
                Cfg.CommitConfig();
            }
        });
    }


    public void ClearEngines()
    {
        _engineManager.ClearAllEngines();
    }

    public void ClearServerContent()
    {
        _contentManager.ClearAll();
    }

    public void OpenLogDirectory()
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = LauncherPaths.DirLogs
        });
    }

    public void OpenAccountSettings()
    {
        Helpers.OpenUri(ConfigConstants.AccountManagementUrl);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class HideLevelDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (HideLevel)(value ?? HideLevel.Normal) switch
        {
            HideLevel.Disabled => "Hidesey is disabled. Servers with engine version 183.0.0 or above will crash the client.",
            HideLevel.Duplicit => "Patcher is hidden from the game. Patches remain visible to allow administrators to inspect which patches are being used.",
            HideLevel.Normal => "Patcher and patches are hidden.",
            HideLevel.Explicit => "Patcher and patches are hidden. Separate patch logging is disabled.",
            HideLevel.Unconditional => "Patcher, patches are hidden. Separate patch logging, Subversion is hidden.",
            _ => "Unknown hide level."
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}

