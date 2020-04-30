using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views
{
    public class MainWindowLogin : UserControl
    {
        public MainWindowLogin()
        {
            InitializeComponent();

            var nameBox = this.FindControl<TextBox>("NameBox");

            nameBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter && DataContext is MainWindowLoginViewModel vm)
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