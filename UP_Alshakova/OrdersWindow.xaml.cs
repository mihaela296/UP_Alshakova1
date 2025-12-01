using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP_Alshakova
{
    public partial class OrdersWindow : Window
    {
        private string _userRole;
        private string _userName;
        private ObservableCollection<dynamic> _orders;

        public OrdersWindow(string userRole, string userName)
        {
            InitializeComponent();
            _userRole = userRole;
            _userName = userName;
            InitializePermissions();
            LoadOrders();
            txtUserInfoHeader.Text = _userName;
        }

        private void InitializePermissions()
        {
            // Только администратор может добавлять/удалять заказы
            if (_userRole == "Admin")
            {
                btnAddOrder.Visibility = Visibility.Visible;
                colActions.Visibility = Visibility.Visible;
            }
            else
            {
                btnAddOrder.Visibility = Visibility.Collapsed;
                colActions.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var context = new Entities())
                {
                    var orders = context.Orders.ToList();

                    _orders = new ObservableCollection<dynamic>(
                        orders.Select(o => new
                        {
                            OrderID = o.OrderID,
                            OrderCode = o.OrderCode,
                            Status = o.Status,
                            DeliveryAddress = o.DeliveryAddress,
                            OrderDate = o.OrderDate,
                            DeliveryDate = o.DeliveryDate
                        }).ToList()
                    );

                    dgOrders.ItemsSource = _orders;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся в соответствующее окно в зависимости от роли
            Window productsWindow = null;

            switch (_userRole)
            {
                case "Admin":
                    productsWindow = new AdminWindow(_userName);
                    break;
                case "Manager":
                    productsWindow = new ManagerWindow(_userName);
                    break;
                case "Client":
                    productsWindow = new ClientWindow(_userName);
                    break;
            }

            if (productsWindow != null)
            {
                productsWindow.Show();
                this.Close();
            }
        }

        private void btnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderEditWindow editWindow = new OrderEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadOrders(); // Обновляем список после добавления
            }
        }

        private void dgOrders_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Редактирование заказа открывается только по двойному клику
            if (_userRole == "Admin" && dgOrders.SelectedItem != null)
            {
                dynamic selectedOrder = dgOrders.SelectedItem;
                OrderEditWindow editWindow = new OrderEditWindow(selectedOrder.OrderID);
                if (editWindow.ShowDialog() == true)
                {
                    LoadOrders(); // Обновляем список после редактирования
                }
            }
        }

        private void btnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int orderId = (int)button.Tag;

                using (var context = new Entities())
                {
                    var order = context.Orders.Find(orderId);
                    if (order != null)
                    {
                        var orderCode = order.OrderCode;

                        if (MessageBox.Show($"Вы уверены, что хотите удалить заказ '{orderCode}'?",
                            "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            DeleteOrder(orderId);
                        }
                    }
                }
            }
        }

        private void DeleteOrder(int orderId)
        {
            try
            {
                using (var context = new Entities())
                {
                    var order = context.Orders.Find(orderId);
                    if (order != null)
                    {
                        // Удаляем связанные OrderItems сначала
                        var orderItems = context.OrderItems.Where(oi => oi.OrderID == orderId);
                        context.OrderItems.RemoveRange(orderItems);

                        // Затем удаляем сам заказ
                        context.Orders.Remove(order);
                        context.SaveChanges();

                        LoadOrders();
                        MessageBox.Show("Заказ успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}