using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;
using SS14.Launcher.Marsey;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        public static List<MarseyPatch> Patches { get; private set; }
        public PatchesTabView()
        {
            InitializeComponent();
            MarseyPatcher.LoadAssemblies();
            Patches = MarseyPatcher.GetPatchList();
            Log.Information($"Got {Patches.Count} patches.");
            this.DataContext = this;
        }

        public void OpenPatchDirectory()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = LauncherPaths.DirPatch
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
