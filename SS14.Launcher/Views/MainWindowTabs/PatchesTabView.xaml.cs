using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public partial class PatchesTabView : UserControl
    {
        public PatchesTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
