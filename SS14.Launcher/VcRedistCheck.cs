using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SS14.Launcher
{
    // Checks that the VC++ 2015 redist, which the game needs, is present.
    public static class VcRedistCheck
    {
        public static void Check()
        {
            if (!OperatingSystem.IsWindows())
                return;

            // ReSharper disable once StringLiteralTypo
            if (NativeLibrary.TryLoad("VCRUNTIME140.dll", out var lib))
            {
                // VC++ 2015 runtime present, we're good.
                // Unload the library I guess since we don't need it.
                NativeLibrary.Free(lib);

                return;
            }

            // We could show this dialog all fancy with Avalonia but I'm lazy so.
            var ret = MessageBoxW(IntPtr.Zero,
                "The game needs the VC++ 2015 redistributable installed, which you do not have.\nWould you like to download the installer for it?",
                "VC++ 2015 redistributable not installed",
                MB_ICONERROR | MB_YESNO);

            if (ret == IDYES)
            {
                Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://aka.ms/vs/16/release/vc_redist.x64.exe",
                        UseShellExecute = true
                    })!
                    .WaitForExit();
            }

            Environment.Exit(1);
        }

        [DllImport("user32.dll")]
        private static extern int MessageBoxW(
            IntPtr hWnd,
            [MarshalAs(UnmanagedType.LPWStr)] string lpText,
            [MarshalAs(UnmanagedType.LPWStr)] string lpCaption,
            uint uType);

        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        private const uint MB_ICONERROR = (uint)0x00000010L;
        private const uint MB_YESNO = (uint)0x00000004L;

        private const int IDYES = 6;
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
    }
}
