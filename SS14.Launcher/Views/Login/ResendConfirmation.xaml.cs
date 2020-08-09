using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.Views.Login
{
    public sealed class ResendConfirmation : UserControl
    {
        public ResendConfirmation()
        {
            InitializeComponent();
        }

        private void InputBoxOnKeyDown(object? sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter && DataContext is ForgotPasswordViewModel vm)
            {
                // vm.OnLogInButtonPressed();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}