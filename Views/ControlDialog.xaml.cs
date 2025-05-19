using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WindowLocker.ViewModels;

namespace WindowLocker.Views
{
    public partial class ControlDialog : Window
    {
        public event Action ShowMainWindow;
        private bool _isInitializing;

        private void UpdateSignageButtonState()
        {
            bool isAnySettingNotApplied = false;

            try
            {
                // 1. 화면 설정 확인
                // Lock Screen Settings
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Personalization"))
                {
                    if (key == null || key.GetValue("NoLockScreen") == null || (int)key.GetValue("NoLockScreen") != 1)
                        isAnySettingNotApplied = true;
                }

                // Screen Saver Settings
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Control Panel\Desktop"))
                {
                    if (key == null || 
                        key.GetValue("ScreenSaveActive") == null || key.GetValue("ScreenSaveActive").ToString() != "0" ||
                        key.GetValue("SCRNSAVE.EXE") == null || key.GetValue("SCRNSAVE.EXE").ToString() != "" ||
                        key.GetValue("ScreenSaverIsSecure") == null || key.GetValue("ScreenSaverIsSecure").ToString() != "0")
                        isAnySettingNotApplied = true;
                }

                // 2. 전원 설정 확인
                // Power Settings
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power"))
                {
                    if (key == null || key.GetValue("CsEnabled") == null || (int)key.GetValue("CsEnabled") != 0)
                        isAnySettingNotApplied = true;
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power"))
                {
                    if (key == null || 
                        key.GetValue("HiberbootEnabled") == null || (int)key.GetValue("HiberbootEnabled") != 0 ||
                        key.GetValue("AwayModeEnabled") == null || (int)key.GetValue("AwayModeEnabled") != 1)
                        isAnySettingNotApplied = true;
                }

                // 3. 데스크톱 설정 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    if (key == null || 
                        key.GetValue("DelayLockInterval") == null || (int)key.GetValue("DelayLockInterval") != 0 ||
                        key.GetValue("LogPixels") == null || (int)key.GetValue("LogPixels") != 96 ||
                        key.GetValue("Win8DpiScaling") == null || (int)key.GetValue("Win8DpiScaling") != 0)
                        isAnySettingNotApplied = true;
                }

                // 4. 작업표시줄 설정 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key == null || key.GetValue("MMTaskbarEnabled") == null || (int)key.GetValue("MMTaskbarEnabled") != 0)
                        isAnySettingNotApplied = true;
                }

                // 5. 멀티 터치 제스처 비활성화 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"))
                {
                    if (key != null) // 이 설정은 모든 PC에 있지 않을 수 있음
                    {
                        if (key.GetValue("ThreeFingerGestures") != null && (int)key.GetValue("ThreeFingerGestures") != 0)
                            isAnySettingNotApplied = true;
                        
                        if (key.GetValue("FourFingerGestures") != null && (int)key.GetValue("FourFingerGestures") != 0)
                            isAnySettingNotApplied = true;
                    }
                }

                // 6. 알림 설정 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Explorer"))
                {
                    if (key == null || key.GetValue("DisableNotificationCenter") == null || (int)key.GetValue("DisableNotificationCenter") != 1)
                        isAnySettingNotApplied = true;
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PushNotifications"))
                {
                    if (key == null || key.GetValue("ToastEnabled") == null || (int)key.GetValue("ToastEnabled") != 0)
                        isAnySettingNotApplied = true;
                }

                // 7. 태블릿 모드 설정 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell"))
                {
                    if (key == null || 
                        key.GetValue("TabletMode") == null || (int)key.GetValue("TabletMode") != 0 ||
                        key.GetValue("SignInMode") == null || (int)key.GetValue("SignInMode") != 1 ||
                        key.GetValue("UseActionCenterExperience") == null || (int)key.GetValue("UseActionCenterExperience") != 0)
                        isAnySettingNotApplied = true;
                }

                // 8. Edge UI 설정 확인
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\EdgeUI"))
                {
                    if (key == null || key.GetValue("AllowEdgeSwipe") == null || (int)key.GetValue("AllowEdgeSwipe") != 0)
                        isAnySettingNotApplied = true;
                }

                // 9. 코타나 설정 확인
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                {
                    if (key == null || key.GetValue("AllowCortana") == null || (int)key.GetValue("AllowCortana") != 0)
                        isAnySettingNotApplied = true;
                }

                // 10. 포커스 어시스트 설정 확인
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Focus Assist"))
                {
                    if (key != null) // 이 설정은 모든 PC에 있지 않을 수 있음
                    {
                        if (key.GetValue("FocusLevelAll") != null && (int)key.GetValue("FocusLevelAll") != 2)
                            isAnySettingNotApplied = true;
                        
                        if (key.GetValue("FocusLevelPriority") != null && (int)key.GetValue("FocusLevelPriority") != 1)
                            isAnySettingNotApplied = true;
                    }
                }

                signageSettingsButton.IsEnabled = isAnySettingNotApplied;
            }
            catch (Exception)
            {
                // 예외 발생 시 버튼 활성화
                signageSettingsButton.IsEnabled = true;
            }
        }

        private async void HackingPreventionToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if ((bool)HackingPreventionToggle.IsChecked)               {
                    await Task.Run(() =>
                    {
                        MainWindow.Instance.sViewModel.EnablePreventHack();
                    });

                    MessageBox.Show(MainWindow.Instance,
						(string)Application.Current.FindResource("MsgHackingPreventionSuccess"),
                        (string)Application.Current.FindResource("MsgSuccess"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        MainWindow.Instance.sViewModel.DisablePreventHack();
                    });

                    MessageBox.Show(MainWindow.Instance,
						(string)Application.Current.FindResource("MsgHackingPreventionDisabled"),
                        (string)Application.Current.FindResource("MsgSuccess"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MainWindow.Instance,
					string.Format((string)Application.Current.FindResource("MsgHackingPreventionError"), ex.Message),
                    (string)Application.Current.FindResource("MsgError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void ApplySignageSettings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.sViewModel.ApplySignageSettings();
            UpdateSignageButtonState();
        }

        private void DetailedSettings_Click(object sender, RoutedEventArgs e)
        {

            ShowMainWindow?.Invoke();
            this.Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Hide();
            }
            base.OnKeyDown(e);
        }

        public ControlDialog()
        {
            InitializeComponent();
            _isInitializing = true;
            
            HackingPreventionToggle.IsChecked = MainWindow.Instance.sViewModel.IsSystemLocked;
            UpdateSignageButtonState();  // Initial button state check
            _isInitializing = false;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    this.Topmost = true;
                //}));

                //Thread.Sleep(200);

                //Dispatcher.Invoke(new Action(() =>
                //{
                //    this.Topmost = false;
                //}));
            }
        }
    }
}