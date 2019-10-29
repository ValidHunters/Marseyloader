using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views
{
    public class ClientUpdaterView : UserControl
    {
        public ClientUpdaterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}