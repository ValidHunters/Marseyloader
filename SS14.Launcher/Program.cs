using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using Serilog;
using SS14.Launcher.Models;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.ViewModels;
using SS14.Launcher.Views;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace SS14.Launcher
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            var cfg = new DataManager();
            cfg.Load();

            LauncherPaths.CreateDirs();

            var logCfg = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console();

            if (cfg.LogLauncher)
            {
                logCfg = logCfg.WriteTo.File(LauncherPaths.PathLauncherLog);
            }

            Log.Logger = logCfg.CreateLogger();

#if DEBUG
            Logger.Sink = new AvaloniaSeriLogger(new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Area}] {Message} ({SourceType} #{SourceHash})\n")
                .CreateLogger());
#endif

            BuildAvaloniaApp().Start((app, args) => AppMain(app, args, cfg), args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args, DataManager cfg)
        {
            var engineManager = new EngineManagerDynamic(cfg);
            var statusCache = new ServerStatusCache();
            var updater = new Updater(cfg, engineManager);

            var viewModel = new MainWindowViewModel(cfg, statusCache, updater, engineManager);
            var window = new MainWindow
            {
                DataContext = viewModel
            };
            viewModel.OnWindowInitialized();

            app.Run(window);
        }
    }
}
