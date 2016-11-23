using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FantasyFootball.Terminal
{
    public class ConsolePrepper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;

        public static void Prep()
        {
            Process proc = Process.GetCurrentProcess();
            ShowWindow(proc.MainWindowHandle, SW_MAXIMIZE);

            Console.WindowWidth = Console.BufferWidth = 192;
            Console.WindowHeight = Console.BufferHeight = 50;
        }
    }
}
