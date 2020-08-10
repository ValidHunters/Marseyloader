using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public class ServerEntryView : UserControl
    {
        private ServerEntryViewModel? _viewModel;

        public ServerEntryView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Control = null;
            }

            _viewModel = DataContext as ServerEntryViewModel;

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
