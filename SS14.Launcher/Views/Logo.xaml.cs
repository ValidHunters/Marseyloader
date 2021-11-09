using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SS14.Launcher.Views;

public partial class Logo : UserControl
{
    public Logo()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}