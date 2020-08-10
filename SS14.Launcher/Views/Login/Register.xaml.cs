using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels.Login;

namespace SS14.Launcher.Views.Login
{
    public class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();

            var nameBox = this.FindControl<TextBox>("NameBox");

            nameBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter && DataContext is LoginViewModel vm)
                {
                    vm.OnLogInButtonPressed();
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
