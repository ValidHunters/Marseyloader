using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using SS14.Launcher.ViewModels;
using SS14.Launcher.Views;

namespace SS14.Launcher
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            var viewModel = new MainWindowViewModel();
            var window = new MainWindow
            {
                DataContext = viewModel
            };
            viewModel.OnWindowInitialized();

            app.Run(window);
        }

        [DllImport("fluidsynth")]
        public static extern IntPtr new_fluid_settings();
    }
}