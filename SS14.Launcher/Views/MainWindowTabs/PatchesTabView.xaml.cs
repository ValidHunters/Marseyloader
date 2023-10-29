using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        public PatchesTabView()
        {
            InitializeComponent();
            DataContext = new PatchesTabViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
