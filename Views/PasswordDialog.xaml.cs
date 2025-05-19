using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Text;
using WindowLocker.Utilities;

namespace WindowLocker.Views
{
    public partial class PasswordDialog : Window
    {
        // 하드코딩된 비밀번호 제거
        // 대신 암호화된 해시값 저장 (SHA256 해시)
        private static readonly string PASSWORD_HASH = "f5c2df0af8ca8869a93fef597b5d809fc245df2d5977df81efada4f024e56e4e"; // turtle04!9 의 SHA256 해시
        private static readonly string PASSWORD2_HASH = "e8adb5e774e26005f31fc9d96bd1eb00b1bf4b0ffc1d969ba4e8fa426c037e7c"; // 098765 의 SHA256 해시

        public string Password { get; private set; }

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show(MainWindow.Instance,
                    (string)Application.Current.FindResource("MsgPasswordRequired"),
                    (string)Application.Current.FindResource("MsgWarning"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Hide();

            // 입력된 비밀번호의 해시와 저장된 해시 비교
            string inputPasswordHash = ComputeSha256Hash(PasswordBox.Password);
            if (inputPasswordHash == PASSWORD_HASH || inputPasswordHash == PASSWORD2_HASH)
            {
                ((App)Application.Current).ShowControlDialog();
            }

            PasswordBox.Password = string.Empty;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OKButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// 문자열의 SHA256 해시를 계산합니다.
        /// </summary>
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}