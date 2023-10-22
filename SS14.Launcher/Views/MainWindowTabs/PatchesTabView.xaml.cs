using System.Collections.Generic;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.Marsey;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        private List<Patch> Patches;
        public PatchesTabView()
        {
            InitializeComponent();
            Patches = MarseyPatcher.GetPatchList();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
