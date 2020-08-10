using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class MainWindowLogin : UserControl
    {
        public MainWindowLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
