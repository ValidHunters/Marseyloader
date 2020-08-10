using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

namespace SS14.Launcher.Views.MainWindowTabs
{
    public class OptionsTabView : UserControl
    {
        public OptionsTabView()
        {
            InitializeComponent();

            var flip = this.FindControl<Button>("Flip");
            flip.Command = ReactiveCommand.Create(() =>
            {
                var window = (Window) VisualRoot;
                window.Classes.Add("DoAFlip");

                DispatcherTimer.RunOnce(() => { window.Classes.Remove("DoAFlip"); }, TimeSpan.FromSeconds(1));
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
