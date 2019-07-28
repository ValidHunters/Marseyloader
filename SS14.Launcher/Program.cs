using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Qml.Net;
using Qml.Net.Runtimes;

namespace SS14.Launcher
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var loc = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            Environment.CurrentDirectory = loc ?? throw new NullReferenceException();

            FixTlsVersions();
            RuntimeManager.FindSuitableQtRuntime();
            //RuntimeManager.DiscoverOrDownloadSuitableQtRuntime();

            QCoreApplication.SetAttribute(ApplicationAttribute.EnableHighDpiScaling, true);
            using (var app = new QGuiApplication(args))
            {
                ss14l_registerRcc("./assets.rcc");
                unsafe
                {
                    ss14l_setIcon((void*) app.Handle, "icon.ico");
                }
                QQuickStyle.SetStyle("Material");
                using (var engine = new QQmlApplicationEngine())
                {
                    Qml.Net.Qml.RegisterType<Launcher>("SS14Launcher");
                    engine.Load("assets/qml/main.qml");
                    app.Exec();
                }
            }
        }

        [Conditional("NET_FRAMEWORK")]
        private static void FixTlsVersions()
        {
            // So, supposedly .NET Framework 4.7 is supposed to automatically select sane TLS versions.
            // Yet, it does not for some people. This causes it to try to connect to our servers with
            // SSL 3 or TLS 1.0, neither of which are accepted for security reasons.
            // (The minimum our servers accept is TLS 1.2)
            // So, ONLY on Windows (Mono is fine) and .NET Framework we manually tell it to use TLS 1.2
            // I assume .NET Core does not have this issue being disconnected from the OS and all that.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
            }
        }

        // ReSharper disable once StringLiteralTypo
        [DllImport("ss14launcherext.dll")]
        private static extern unsafe void ss14l_setIcon(void* app, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

        // ReSharper disable once StringLiteralTypo
        [DllImport("ss14launcherext.dll")]
        private static extern unsafe void ss14l_registerRcc([MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    }
}