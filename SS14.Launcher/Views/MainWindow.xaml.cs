using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Win32;
using SS14.Launcher.ViewModels;
using TerraFX.Interop.Windows;

namespace SS14.Launcher.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        DarkMode();

#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Control = null;
        }

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel != null)
        {
            _viewModel.Control = this;
        }

        base.OnDataContextChanged(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private unsafe void DarkMode()
    {
        if (PlatformImpl is not WindowImpl windowImpl || Environment.OSVersion.Version.Build < 22000)
            return;

        var type = windowImpl.GetType();
        var prop = type.GetProperty("Hwnd", BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop == null)
        {
            // No need to log a warning, PJB will notice when this breaks.
            return;
        }

        var hWnd = (HWND)(nint)prop.GetValue(windowImpl)!;

        COLORREF r = 0x00262121;
        Windows.DwmSetWindowAttribute(hWnd, 35, &r, (uint) sizeof(COLORREF));
    }
}
