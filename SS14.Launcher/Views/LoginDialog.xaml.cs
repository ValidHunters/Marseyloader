using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace SS14.Launcher.Views
{
    public class LoginDialog : Window
    {
        private readonly TextBox _nameBox;
        private readonly TextBlock _invalidReason;
        private readonly Button _submitButton;
        private string? _defaultName;

        public LoginDialog()
        {
            InitializeComponent();

            _nameBox = this.FindControl<TextBox>("NameBox");
            _nameBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    TrySubmit();
                }
            };

            _invalidReason = this.FindControl<TextBlock>("InvalidReason");

            _submitButton = this.FindControl<Button>("SubmitButton");
            _submitButton.Command = ReactiveCommand.Create(TrySubmit);

            this.WhenAnyValue(x => x._nameBox.Text)
                .Subscribe(UpdateSubmitValid);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            _nameBox.Focus();
        }

        public string? DefaultName
        {
            get => _defaultName;
            set
            {
                _defaultName = value;
                _nameBox.Text = value;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TrySubmit()
        {
            if (!UsernameHelpers.IsNameValid(_nameBox.Text, out _))
            {
                return;
            }

            Close(_nameBox.Text);
        }

        private void UpdateSubmitValid(string newText)
        {
            var valid = UsernameHelpers.IsNameValid(newText, out var reason);

            _submitButton.IsEnabled = valid;
            _invalidReason.Text = valid ? null : reason;
        }
    }
}