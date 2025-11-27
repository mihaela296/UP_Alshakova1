using System.Windows;
using System.Data.Entity;
using System.Linq;

namespace UP_Alshakova
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                RegisterUser();
            }
        }

        private bool ValidateData()
        {
            HideError();

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Введите ФИО");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                ShowError("Введите логин");
                return false;
            }

            if (txtPassword.Password.Length < 4)
            {
                ShowError("Пароль должен содержать минимум 4 символа");
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают");
                return false;
            }

            // Проверка уникальности логина
            using (var context = new Entities())
            {
                if (context.Users.Any(u => u.Login == txtLogin.Text.Trim()))
                {
                    ShowError("Пользователь с таким логином уже существует");
                    return false;
                }
            }

            return true;
        }

        private void RegisterUser()
        {
            try
            {
                using (var context = new Entities())
                {
                    var newUser = new User
                    {
                        FullName = txtFullName.Text.Trim(),
                        Login = txtLogin.Text.Trim(),
                        Password = txtPassword.Password,
                        RoleID = 1 // Роль "Client" по умолчанию
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
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