using System;
using Avalonia;
using Avalonia.Controls;
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
    }
}
