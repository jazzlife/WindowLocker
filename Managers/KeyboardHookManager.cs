using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowLocker.Utilities;

namespace WindowLocker.Managers
{
    public class KeyboardHookManager
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Action _f7Action;

        public static bool WinBlocked { get; set; } = false;

        public static void StartHook(Action f7Action)
        {
            _f7Action = f7Action;
            _hookID = SetHook(_proc);
        }

        public static void StopHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == Keys.F7)
                {
                    _f7Action?.Invoke();
                }else if(WinBlocked)
                {
                    // Windows 키 단독 입력 차단
                    if (vkCode == Win32Api.VK_LWIN || vkCode == Win32Api.VK_RWIN)
                    {
                        return (IntPtr)1;
                    }

                    // Windows 키 조합 차단 (예: Win+R, Win+E 등)
                    if (Win32Api.GetAsyncKeyState(Win32Api.VK_LWIN) < 0 ||
                        Win32Api.GetAsyncKeyState(Win32Api.VK_RWIN) < 0)
                    {
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}