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
        private static readonly string PASSWORD_HASH = "112ca3421e26622302e7a52a6c17e0388f1f7fa465c297fcfd114c287c0fb089";
        private static readonly string PASSWORD2_HASH = "4a9ca4596692e94f9d2912b06a0d007564a22ee750339a6021c2392149b25d6d";

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

            // 표준화된 해시 생성 메서드 사용
            string inputPasswordHash = HashGenerator.ComputePasswordHash(PasswordBox.Password);
                        
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
    }
}