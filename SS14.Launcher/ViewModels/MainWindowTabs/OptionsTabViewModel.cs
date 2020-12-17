using System.IO;
using System.Linq;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class OptionsTabViewModel : MainWindowTabViewModel
    {
        public readonly DataManager Cfg;
        private readonly IEngineManager _engineManager;

        public OptionsTabViewModel(DataManager cfg, IEngineManager engineManager)
        {
            Cfg = cfg;
            _engineManager = engineManager;
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

        public void ClearEngines()
        {
            _engineManager.ClearAllEngines();
        }

        public void ClearServerContent()
        {
            foreach (var content in Cfg.ServerContent.Items.ToArray())
            {
                Cfg.RemoveInstallation(content);
            }

            foreach (var file in Directory.EnumerateFiles(LauncherPaths.DirServerContent))
            {
                File.Delete(file);
            }
        }
    }
}
