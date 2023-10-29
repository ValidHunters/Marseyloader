using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Marsey;

namespace SS14.Launcher.ViewModels.MainWindowTabs
{
    public class PatchesTabViewModel : MainWindowTabViewModel
    {
        public override string Name => "Plugins";

        public ObservableCollection<MarseyPatch> Patches { get; private set; }

        public ICommand OpenPatchDirectoryCommand { get; private set; }

        public PatchesTabViewModel()
        {
            Patches = new ObservableCollection<MarseyPatch>();
            OpenPatchDirectoryCommand = new RelayCommand(OpenPatchDirectory);
            LoadPatches();
        }

        private void LoadPatches()
        {
            FileHandler.LoadAssemblies();
            var patches = PatchAssemblyManager.GetPatchList();
            foreach (var patch in patches)
            {
                Patches.Add(patch);
            }
            Log.Debug($"Refreshed patches, got {Patches.Count}.");
        }

        private void OpenPatchDirectory()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "Marsey")
            });
        }
    }
}
