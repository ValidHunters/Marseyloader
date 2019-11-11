using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace SS14.Launcher.Views
{
    public class OutOfDateDialog : Window
    {
        public OutOfDateDialog()
        {
            InitializeComponent();

            this.FindControl<Button>("DownloadButton").Command = ReactiveCommand.Create(DownloadPressed);
            this.FindControl<Button>("ExitButton").Command = ReactiveCommand.Create(ExitPressed);
        }

        private static void DownloadPressed()
        {
            Helpers.OpenUri(new Uri("https://spacestation14.io/about/nightlies/"));
        }

        private void ExitPressed()
        {
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}