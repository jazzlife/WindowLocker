using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using WindowLocker.Managers;
using WindowLocker.Utilities;
using WindowLocker.Views;
using frms = System.Windows.Forms;

namespace WindowLocker
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;
        private ControlDialog _controlDialog;
        private PasswordDialog _passwordDialog;
        private frms.NotifyIcon _notifyIcon;

        private Mutex _instanceMutex = null;
        string procName = "WindowLocker";

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                bool createdNew;
                _instanceMutex = new Mutex(true, procName, out createdNew);

                if (createdNew)
                {
                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show("Application is already running!");
                    Application.Current.Shutdown();
                    return;
                }

                base.OnStartup(e);

                bool _isAdmin = SystemUtilities.IsRunningAsAdmin();
                if (!_isAdmin)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        UseShellExecute = true,
                        Verb = "runas" // This requests elevation
                    };

                    Process.Start(startInfo);

                    // Exit the current process after starting the new one
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Application.Current.Shutdown();
                    }));
                    return;
                }

                InitializeNotifyIcon();
                InitializeKeyboardHook();

                // Hide main window on startup
                _mainWindow = new MainWindow();
                _mainWindow.Closing += MainWindow_Closing;

                _controlDialog = new ControlDialog();
                _controlDialog.ShowMainWindow += () =>
                {
                    _mainWindow.Show();
                    _mainWindow.Activate();
                };

                _passwordDialog = new PasswordDialog();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Application.Current.Shutdown();
                return;
            }
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new frms.NotifyIcon
            {
                Icon = new Icon(System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Resources/icon.ico")).Stream),
                Visible = true
            };

            _notifyIcon.DoubleClick += ShowPwDlg;
        }

        private void InitializeKeyboardHook()
        {
            KeyboardHookManager.StartHook(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowPasswordDialog();
                });
            });
        }

        private void ShowPasswordDialog()
        {
            if (_mainWindow.IsVisible || _controlDialog.IsVisible || _passwordDialog.IsVisible)
                return;

            _passwordDialog.Show();
            _passwordDialog.Activate();
            _passwordDialog.PasswordBox.Focus();
        }

        public void ShowControlDialog()
        {
            _controlDialog.Show();
            _controlDialog.Activate();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            _mainWindow.Hide();
            ((App)Application.Current).ShowControlDialog();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.DoubleClick -= ShowPwDlg;
                _notifyIcon.Dispose();
            }

            KeyboardHookManager.StopHook();
            SingleInstanceManager.Release();
            
            base.OnExit(e);
        }

        private void ShowPwDlg(object sender, EventArgs e)
        {
            ShowPasswordDialog();
        }
    }
}
