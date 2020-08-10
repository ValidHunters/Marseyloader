using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views
{
    public class ConnectingDialog : Window
    {
        public ConnectingDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
