using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class OptionsTabViewModel : MainWindowTabViewModel
    {
        public DataManager Cfg { get; }
        private readonly IEngineManager _engineManager;

        public OptionsTabViewModel(DataManager cfg, IEngineManager engineManager)
        {
            Cfg = cfg;
            _engineManager = engineManager;
        }

#if RELEASE
        public bool HideDisableSigning => true;
#else
        public bool HideDisableSigning => false;
#endif

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

        public void OpenLogDirectory()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = LauncherPaths.DirLogs
            });
        }
    }
}
