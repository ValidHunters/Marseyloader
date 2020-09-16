using SS14.Launcher.Models;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class OptionsTabViewModel : MainWindowTabViewModel
    {
        public readonly DataManager Cfg;

        public OptionsTabViewModel(DataManager cfg)
        {
            Cfg = cfg;
        }

        public bool ForceGLES2
        {
            get
            {
                return Cfg.ForceGLES2;
            }
            set
            {
                Cfg.ForceGLES2 = value;
            }
        }

        public override string Name => "Options";
    }
}
