using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using WindowLocker.Utilities;

namespace WindowLocker.Managers
{
    public class DesktopManager
    {
        private static string _originalWallpaper;

        public static void SetWallpaper(string color)
        {
            try
            {
                // Store original wallpaper path
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
                {
                    if (key != null)
                    {
                        _originalWallpaper = (string)key.GetValue("Wallpaper");
                        Console.WriteLine($"Original wallpaper path saved: {_originalWallpaper}");
                    }
                }

                // Set solid color as wallpaper
                Win32Api.SystemParametersInfo(
                    Win32Api.SPI_SETDESKWALLPAPER, 
                    0, 
                    color, // 색상 값을 직접 전달
                    Win32Api.SPIF_UPDATEINIFILE | Win32Api.SPIF_SENDCHANGE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting wallpaper: {ex.Message}");
                throw;
            }
        }

        public static void RestoreDefaultWallpaper()
        {
            try
            {
                if (!string.IsNullOrEmpty(_originalWallpaper))
                {
                    Console.WriteLine($"Restoring wallpaper to: {_originalWallpaper}");

                    if (File.Exists(_originalWallpaper))
                    {
                        // 원래 배경화면 복원
                        Win32Api.SystemParametersInfo(
                            Win32Api.SPI_SETDESKWALLPAPER, 
                            0, 
                            _originalWallpaper, 
                            Win32Api.SPIF_UPDATEINIFILE | Win32Api.SPIF_SENDCHANGE);
                    }
                    else
                    {
                        Console.WriteLine("Original wallpaper file not found");
                    }
                    _originalWallpaper = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring wallpaper: {ex.Message}");
                throw;
            }
        }

        public static void SetDesktopIconsVisibility(bool show)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null)
                    {
                        key.SetValue("HideIcons", show ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                IntPtr progman = Win32Api.FindWindow("Progman", "Program Manager");
                if (progman != IntPtr.Zero)
                {
                    Win32Api.PostMessage(progman, Win32Api.WM_COMMAND, new IntPtr(Win32Api.ID_REFRESH), IntPtr.Zero);

                    IntPtr workerW = Win32Api.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (workerW != IntPtr.Zero)
                    {
                        IntPtr folderView = Win32Api.FindWindowEx(workerW, IntPtr.Zero, "SysListView32", "FolderView");
                        if (folderView != IntPtr.Zero)
                        {
                            Win32Api.ShowWindow(folderView, show ? Win32Api.SW_SHOW : Win32Api.SW_HIDE);
                        }
                    }
                }

                IntPtr currentWorkerW = Win32Api.FindWindow("WorkerW", null);
                while (currentWorkerW != IntPtr.Zero)
                {
                    IntPtr shellView = Win32Api.FindWindowEx(currentWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (shellView != IntPtr.Zero)
                    {
                        IntPtr folderView = Win32Api.FindWindowEx(shellView, IntPtr.Zero, "SysListView32", "FolderView");
                        if (folderView != IntPtr.Zero)
                        {
                            Win32Api.ShowWindow(folderView, show ? Win32Api.SW_SHOW : Win32Api.SW_HIDE);
                        }
                    }
                    currentWorkerW = Win32Api.FindWindowEx(IntPtr.Zero, currentWorkerW, "WorkerW", null);
                }

                Win32Api.SHChangeNotify(Win32Api.SHCNE_ASSOCCHANGED, Win32Api.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                Thread.Sleep(100);
                Win32Api.PostMessage(progman, Win32Api.WM_COMMAND, new IntPtr(Win32Api.ID_REFRESH), IntPtr.Zero);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set desktop icons visibility: {ex.Message}", ex);
            }
        }
    }
}