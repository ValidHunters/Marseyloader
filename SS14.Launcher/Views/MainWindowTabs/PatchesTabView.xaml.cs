using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;
using SS14.Launcher.Marsey;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        public static List<Patch> Patches { get; private set; }
        public PatchesTabView()
        {
            InitializeComponent();
            Patches = MarseyPatcher.GetPatchList();
            Log.Information($"Got {Patches.Count} patches.");
            this.DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
