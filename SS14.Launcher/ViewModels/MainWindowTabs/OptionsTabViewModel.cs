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
using Marsey.Misc;
using Marsey.Stealthsey;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Splat;
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
    public ICommand SetUsernameCommand { get; }
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
        SetUsernameCommand = new RelayCommand(OnSetUsernameClick);
        SetEndpointCommand = new RelayCommand(OnSetEndpointClick);
    }

#if RELEASE
        public bool HideDisableSigning => true;
#else
    public bool HideDisableSigning => false;
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
            Cfg.CommitConfig();
        }
    }

    private string _endpoint = "";
    public string MarseyApiEndpoint
    {
        get => Cfg.GetCVar(CVars.MarseyApiEndpoint);
        set => _endpoint = value;
    }

    public bool ForcingHWid
    {
        get => Cfg.GetCVar(CVars.ForcingHWId);
        set
        {
            Cfg.SetCVar(CVars.ForcingHWId, value);
            Cfg.CommitConfig();
        }
    }

    private string _HWIdString = "";
    public string HWIdString
    {
        get => Cfg.GetCVar(CVars.ForcedHWId);
        set => _HWIdString = value;
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
        // Check if _HWIdString is a valid hex string (allowing empty string) and pad it if necessary
        if (Regex.IsMatch(_HWIdString, "^[a-fA-F0-9]*$")) // '*' allows for zero or more characters
        {
        
            Cfg.SetCVar(CVars.ForcedHWId, _HWIdString);
            Cfg.CommitConfig();
        }
        else
        {
            Log.Warning("Passed HWId is not a valid hexadecimal string! Refusing to apply.");
        }
    }


    
    private void OnSetUsernameClick()
    {
        _dataManager.ChangeLogin(ChangeReason.Update, _loginManager.ActiveAccount?.LoginInfo!);
        _dataManager.CommitConfig();
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
            HideLevel.Explicit => "Patcher and patches are hidden. Patch logging is disabled.",
            HideLevel.Unconditional => "Patcher, patches are hidden. Patch logging, subversion and preloads are disabled.",
            _ => "Unknown hide level."
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}

