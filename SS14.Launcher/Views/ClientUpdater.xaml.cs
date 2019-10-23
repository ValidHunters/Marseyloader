using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views
{
    public class ClientUpdater : UserControl
    {
        public ClientUpdater()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}