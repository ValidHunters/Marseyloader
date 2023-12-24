using ReactiveUI;
using Splat;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed class DevelopmentTabViewModel : MainWindowTabViewModel
{
    public DataManager Cfg { get; }

    public DevelopmentTabViewModel()
    {
        Cfg = Locator.Current.GetRequiredService<DataManager>();

        // TODO: This sucks and leaks.
        Cfg.GetCVarEntry(CVars.EngineOverrideEnabled).PropertyChanged += (sender, args) =>
        {
            this.RaisePropertyChanged(nameof(Name));
        };
    }

    public override string Name => Cfg.GetCVar(CVars.EngineOverrideEnabled) ? "[DEV (override active!!!)]" : "[DEV]";

    public bool DisableSigning
    {
        get => Cfg.GetCVar(CVars.DisableSigning);
        set
        {
            Cfg.SetCVar(CVars.DisableSigning, value);
            Cfg.CommitConfig();
        }
    }

    public bool EngineOverrideEnabled
    {
        get => Cfg.GetCVar(CVars.EngineOverrideEnabled);
        set
        {
            Cfg.SetCVar(CVars.EngineOverrideEnabled, value);
            Cfg.CommitConfig();
        }
    }

    public string EngineOverridePath
    {
        get => Cfg.GetCVar(CVars.EngineOverridePath);
        set
        {
            Cfg.SetCVar(CVars.EngineOverridePath, value);
            Cfg.CommitConfig();
        }
    }
}
