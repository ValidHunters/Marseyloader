using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views
{
    public class NewsTabView : UserControl
    {
        public NewsTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}