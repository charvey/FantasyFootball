using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FantasyFootball.Terminal.System
{
    public class ConsolePrepper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;

        public static void Maximize(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_MAXIMIZE);
        }

        public static void Prep()
        {
            Process proc = Process.GetCurrentProcess();
            Maximize(proc.MainWindowHandle);

            //Console.WindowWidth = Console.BufferWidth = 192;
            //Console.WindowHeight = Console.BufferHeight = 50;
        }
    }
}
