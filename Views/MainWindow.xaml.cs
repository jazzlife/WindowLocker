using System.Windows;
using System.Windows.Controls;
using WindowLocker.Managers;
using WindowLocker.ViewModels;
using System.Linq;
using WindowLocker.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WindowLocker
{
    public partial class MainWindow : Window
    {
        private PasswordBox _autoLoginPasswordBox;
        private List<IntPtr> _monitors = new List<IntPtr>();

        public static MainWindow Instance { get; private set; }

        public MainViewModel sViewModel = new MainViewModel();

        public MainWindow()
        {
            this.DataContext = sViewModel;
            InitializeComponent();
            Instance = this;

            // Initialize language
            LanguageManager.InitializeLanguage();
            
            // Set saved language selection in LanguageComboBox
            string savedLanguage = Properties.Settings.Default.Language;
            LanguageComboBox.SelectedItem = LanguageComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == savedLanguage);

            // 자동 로그인 설정 불러오기
            _autoLoginPasswordBox = this.FindName("AutoLoginPasswordBox") as PasswordBox;
            if (_autoLoginPasswordBox != null)
            {
                if (sViewModel != null)
                {
                    sViewModel.AutoLoginPasswordBox = _autoLoginPasswordBox;
                    
                    // 마지막 저장된 비밀번호 로드
                    string savedPassword = Properties.Settings.Default.AutoLoginPassword;
                    if (!string.IsNullOrEmpty(savedPassword))
                    {
                        _autoLoginPasswordBox.Password = savedPassword;
                    }
                }
            }

            sViewModel.ShowRestartAsAdmin = SystemUtilities.IsRunningAsAdmin();

            SystemManager.SetTaskbarEnabled(!sViewModel.HideTaskbar);
            
            // 모니터 정보 초기화
            EnumerateMonitors();
            
            // 키보드 이벤트 핸들러 등록
            this.KeyDown += MainWindow_KeyDown;
        }
        
        private bool EnumerateMonitors()
        {
            _monitors.Clear();
            bool result = Win32Api.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
            return result;
        }
        
        private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Win32Api.RECT lprcMonitor, IntPtr dwData)
        {
            _monitors.Add(hMonitor);
            return true;
        }
        
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+숫자키 처리
            if (Keyboard.Modifiers == ModifierKeys.Control && 
                ((e.Key >= Key.D1 && e.Key <= Key.D9) || (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)))
            {
                int monitorIndex = 0;
                
                // 숫자키 값 추출 (D1~D9 또는 NumPad1~NumPad9)
                if (e.Key >= Key.D1 && e.Key <= Key.D9)
                {
                    monitorIndex = (int)e.Key - (int)Key.D1;
                }
                else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
                {
                    monitorIndex = (int)e.Key - (int)Key.NumPad1;
                }
                
                // 지정된 모니터로 창 이동
                MoveToMonitor(monitorIndex);
                
                e.Handled = true;
            }
        }
        
        private void MoveToMonitor(int monitorIndex)
        {
            // 모니터 범위 확인
            if (monitorIndex < 0 || monitorIndex >= _monitors.Count)
            {
                return;
            }
            
            // 선택된 모니터의 정보 가져오기
            IntPtr hMonitor = _monitors[monitorIndex];
            Win32Api.MONITORINFO monitorInfo = new Win32Api.MONITORINFO();
            monitorInfo.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Api.MONITORINFO));
            
            if (Win32Api.GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                // 모니터의 작업 영역 계산
                int width = monitorInfo.rcWork.right - monitorInfo.rcWork.left;
                int height = monitorInfo.rcWork.bottom - monitorInfo.rcWork.top;

                // 창 크기 계산 (모니터 작업 영역의 80%로 설정)
                int windowWidth = (int)this.Width;
                int windowHeight = (int)(this.Height);
                
                // 창 위치 계산 (모니터 작업 영역 중앙)
                int left = monitorInfo.rcWork.left + (width - windowWidth) / 2;
                int top = monitorInfo.rcWork.top + (height - windowHeight) / 2;
                
                // 창의 핸들 가져오기
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                
                // 창 위치 및 크기 설정
                Win32Api.SetWindowPos(hwnd, IntPtr.Zero, left, top, windowWidth, windowHeight, 
                    Win32Api.SWP_NOZORDER | Win32Api.SWP_NOACTIVATE);
                
                // 메시지 표시 (선택적)
                this.Title = $"WindowLocker - 모니터 {monitorIndex + 1}";
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureName = selectedItem.Tag.ToString();
                LanguageManager.SwitchLanguage(cultureName);
            }
        }

        private void AutoLoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sViewModel != null)
                sViewModel.UpdateAutoLoginPassword();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                //if(sViewModel != null)
                //{
                //    sViewModel.InitializeProperties();
                //}
                
                // 창이 표시될 때마다 모니터 목록 갱신
                EnumerateMonitors();
            }
        }
    }
}