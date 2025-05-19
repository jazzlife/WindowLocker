using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WindowLocker.Views
{
    public partial class PasswordDialog : Window
    {
        private const string PASSWORD = "turtle04!9"; // 내부 사용 암호
        private const string PASSWORD2 = "098765"; // 외부 사용 암호

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

            string _pw = PasswordBox.Password;
            if (_pw == PASSWORD || _pw == PASSWORD2)
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