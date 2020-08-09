using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
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

#if DEBUG
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.F12)
            {
                this.OpenDevTools();
            }
        }
#endif

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}