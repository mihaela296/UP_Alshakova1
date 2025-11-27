using System.Windows;
using System.Data.Entity;
using System.Linq;

namespace UP_Alshakova
{
    public partial class UserProfileWindow : Window
    {
        private int _userId;

        public UserProfileWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                using (var context = new Entities())
                {
                    var user = context.Users
                        .Include(u => u.Role)
                        .FirstOrDefault(u => u.UserID == _userId);

                    if (user != null)
                    {
                        txtFullName.Text = user.FullName;
                        txtLogin.Text = $"Логин: {user.Login}";
                        txtRole.Text = $"Роль: {user.Role.RoleName}";
                        txtUserID.Text = $"ID пользователя: {user.UserID}";
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(_userId);
            if (changePasswordWindow.ShowDialog() == true)
            {
                MessageBox.Show("Пароль успешно изменен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}