using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class HomePageView : UserControl
    {
        private HomePageViewModel _viewModel;

        public HomePageView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Control = null;
            }

            _viewModel = DataContext as HomePageViewModel;

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