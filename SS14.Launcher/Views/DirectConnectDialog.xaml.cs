using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace SS14.Launcher.Views
{
    public class DirectConnectDialog : Window
    {
        private readonly TextBox _addressBox;

        public DirectConnectDialog()
        {
            InitializeComponent();

            _addressBox = this.FindControl<TextBox>("AddressBox");
            _addressBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    TrySubmit();
                }
            };

            var submitButton = this.FindControl<Button>("SubmitButton");
            submitButton.Command = ReactiveCommand.Create(TrySubmit);

            _addressBox.WhenAnyValue(x => x.Text)
                .Select(IsAddressValid)
                .Subscribe(b => submitButton.IsEnabled = b);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TrySubmit()
        {
            if (!IsAddressValid(_addressBox.Text))
            {
                return;
            }

            Close(_addressBox.Text);
        }

        private static bool IsAddressValid(string address)
        {
            return !string.IsNullOrWhiteSpace(address);
        }
    }
}