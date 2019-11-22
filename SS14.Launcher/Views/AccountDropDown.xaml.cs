using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ReactiveUI;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class AccountDropDown : UserControl
    {
        private AccountDropDownViewModel? _viewModel;
        private readonly Popup _popup;
        private readonly ToggleButton _button;

        public AccountDropDown()
        {
            InitializeComponent();

            _popup = this.FindControl<Popup>("Popup");
            _button = this.FindControl<ToggleButton>("Button");

            this.WhenAnyValue(x => x._button.IsChecked)
                .Subscribe(n =>
                {
                    if (n == true)
                    {
                        _popup.Open();
                    }
                    else
                    {
                        _popup.Close();
                    }
                });

            _popup.Closed += (sender, args) => _button.IsChecked = false;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Control = null;
            }

            _viewModel = DataContext as AccountDropDownViewModel;

            if (_viewModel != null)
            {
                _viewModel.Control = this;
            }

            base.OnDataContextChanged(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!e.Handled)
            {
                if (_popup?.IsInsidePopup((IVisual) e.Source) == false)
                {
                    _button.IsChecked = !_button.IsChecked;
                    e.Handled = true;
                }
            }

            base.OnPointerPressed(e);
        }
    }
}