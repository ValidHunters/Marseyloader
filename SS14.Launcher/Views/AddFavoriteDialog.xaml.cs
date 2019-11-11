using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace SS14.Launcher.Views
{
    public class AddFavoriteDialog : Window
    {
        private readonly TextBox _nameBox;
        private readonly TextBox _addressBox;
        private readonly Button _submitButton;

        public AddFavoriteDialog()
        {
            InitializeComponent();

            _nameBox = this.FindControl<TextBox>("NameBox");
            _addressBox = this.FindControl<TextBox>("AddressBox");

            _submitButton = this.FindControl<Button>("SubmitButton");
            _submitButton.Command = ReactiveCommand.Create(TrySubmit);

            this.WhenAnyValue(x => x._nameBox.Text)
                .Subscribe(_ => UpdateSubmitValid());

            this.WhenAnyValue(x => x._addressBox.Text)
                .Subscribe(_ => UpdateSubmitValid());
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            _nameBox.Focus();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TrySubmit()
        {
            Close((_nameBox.Text, _addressBox.Text));
        }

        private void UpdateSubmitValid()
        {
            var valid = DirectConnectDialog.IsAddressValid(_addressBox.Text) && !string.IsNullOrEmpty(_nameBox.Text);

            _submitButton.IsEnabled = valid;
        }
    }
}