﻿using System;
using System.Runtime.InteropServices;

namespace TurtleTools
{
    public class Taskbar
    {
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        [DllImport("user32.dll")]
        public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

        [DllImport("user32.dll")]
        private static extern int GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(int hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        protected static int Handle
        {
            get
            {
                return FindWindow("Shell_TrayWnd", "");
            }
        }

        protected static int HandleOfStartButton
        {
            get
            {
                int handleOfDesktop = GetDesktopWindow();
                int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
                return handleOfStartButton;
            }
        }

        private Taskbar()
        {
            // hide ctor
        }

        public static void Show()
        {
            ShowWindow(Handle, SW_SHOW);
            ShowWindow(HandleOfStartButton, SW_SHOW);
        }

        public static void Hide()
        {
            ShowWindow(Handle, SW_HIDE);
            ShowWindow(HandleOfStartButton, SW_HIDE);
        }

        public static bool IsTaskbarVisible()
        {
            int hTaskbar = FindWindow("Shell_TrayWnd", null);
            if (hTaskbar == 0)
            {
                Console.WriteLine("Taskbar not found!");
                return false;
            }
            return IsWindowVisible(hTaskbar);
        }
    }
}
