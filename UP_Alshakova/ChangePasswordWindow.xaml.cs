using System.Windows;
using System.Data.Entity;
using System.Linq;

namespace UP_Alshakova
{
    public partial class ChangePasswordWindow : Window
    {
        private int _userId;

        public ChangePasswordWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                ChangePassword();
            }
        }

        private bool ValidateData()
        {
            HideError();

            if (string.IsNullOrWhiteSpace(txtOldPassword.Password))
            {
                ShowError("Введите старый пароль");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNewPassword.Password))
            {
                ShowError("Введите новый пароль");
                return false;
            }

            if (txtNewPassword.Password.Length < 4)
            {
                ShowError("Новый пароль должен содержать минимум 4 символа");
                return false;
            }

            if (txtNewPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Новые пароли не совпадают");
                return false;
            }

            // Проверка старого пароля
            using (var context = new Entities())
            {
                var user = context.Users.Find(_userId);
                if (user == null || user.Password != txtOldPassword.Password)
                {
                    ShowError("Старый пароль неверен");
                    return false;
                }
            }

            return true;
        }

        private void ChangePassword()
        {
            try
            {
                using (var context = new Entities())
                {
                    var user = context.Users.Find(_userId);
                    if (user != null)
                    {
                        user.Password = txtNewPassword.Password;
                        context.SaveChanges();

                        MessageBox.Show("Пароль успешно изменен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка смены пароля: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }
    }
}