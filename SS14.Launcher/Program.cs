using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Serilog;
using Serilog.Events;
using SS14.Launcher.Models;
using SS14.Launcher.Models.ServerStatus;
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

#if DEBUG
            SerilogLogger.Initialize(new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Area}] {Message} ({SourceType} #{SourceHash})\n")
                .CreateLogger());
#endif

            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            var cfg = new DataManager();
            cfg.Load();
            var statusCache = new ServerStatusCache();
            var updater = new Updater(cfg);

            var viewModel = new MainWindowViewModel(cfg, statusCache, updater);
            var window = new MainWindow
            {
                DataContext = viewModel
            };
            viewModel.OnWindowInitialized();

            app.Run(window);
        }
    }
}
