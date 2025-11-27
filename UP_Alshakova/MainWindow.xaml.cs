using System;
using System.Data.SqlClient;
using System.Windows;
using UP_Alshakova;

namespace ShoeStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            // Проверка авторизации в БД
            string role = CheckAuthorization(login, password);

            if (role != null)
            {
                string fullName = GetUserFullName(login);
                OpenRoleWindow(role, fullName);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            GuestWindow guestWindow = new GuestWindow();
            guestWindow.Show();
            this.Close();
        }

        // Общая строка подключения для всех методов
        private string GetConnectionString()
        {
            return "data source=OOO_ObuvDorogan.mssql.somee.com;initial catalog=OOO_ObuvDorogan;user id=mihaela_d_SQLLogin_1;password=uxql9lvgbv;encrypt=True;trustservercertificate=True;MultipleActiveResultSets=True;";
        }

        private string CheckAuthorization(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                // Предполагая, что вам нужно получить роль пользователя
                string query = "SELECT r.RoleName FROM Users u JOIN Roles r ON u.RoleID = r.RoleID WHERE u.Login = @Login AND u.Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private string GetUserFullName(string login)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT FullName FROM Users WHERE Login = @Login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private int GetUserId(string login)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT UserID FROM Users WHERE Login = @Login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private void OpenRoleWindow(string role, string fullName)
        {
            Window window = null;
            int userId = GetUserId(txtLogin.Text);

            switch (role)
            {
                case "Client":
                    window = new ClientWindow(fullName, userId);
                    break;
                case "Manager":
                    window = new ManagerWindow(fullName, userId);
                    break;
                case "Admin":
                    window = new AdminWindow(fullName, userId);
                    break;
            }

            if (window != null)
            {
                window.Show();
                this.Close();
            }
        }
    }
}