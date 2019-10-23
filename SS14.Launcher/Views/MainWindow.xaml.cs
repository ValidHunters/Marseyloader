using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}