using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;
using SS14.Launcher.Marsey;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        public static List<MarseyPatch>? Patches { get; private set; }
        public PatchesTabView()
        {
            InitializeComponent();
            MarseyPatcher.LoadAssemblies();
            Patches = MarseyPatcher.GetPatchList();
            Log.Debug($"Refreshed patches, got {Patches.Count}.");
            this.DataContext = this;
        }

        public void OpenPatchDirectory()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "Marsey")
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
