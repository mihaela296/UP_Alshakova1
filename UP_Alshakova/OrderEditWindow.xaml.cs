using System.Windows;
using System.Data.Entity;
using System.Linq;

namespace UP_Alshakova
{
    public partial class OrderEditWindow : Window
    {
        private int _orderId;
        private bool _isEditMode;

        public OrderEditWindow(int orderId = 0)
        {
            InitializeComponent();
            _orderId = orderId;
            _isEditMode = orderId > 0;

            InitializeWindow();

            if (_isEditMode)
            {
                LoadOrderData();
            }
            else
            {
                // Установка значений по умолчанию для нового заказа
                dpOrderDate.SelectedDate = System.DateTime.Today;
                cmbStatus.SelectedIndex = 0; // "Новый"
            }
        }

        private void InitializeWindow()
        {
            if (_isEditMode)
            {
                txtWindowTitle.Text = "Редактирование заказа";
            }
            else
            {
                txtWindowTitle.Text = "Добавление заказа";

                // Генерация артикула для нового заказа
                using (var context = new Entities())
                {
                    int lastOrderId = context.Orders.Any() ? context.Orders.Max(o => o.OrderID) : 0;
                    txtOrderCode.Text = $"ORD{(lastOrderId + 1).ToString().PadLeft(6, '0')}";
                }
            }
        }

        private void LoadOrderData()
        {
            try
            {
                using (var context = new Entities())
                {
                    var order = context.Orders.FirstOrDefault(o => o.OrderID == _orderId);

                    if (order != null)
                    {
                        txtOrderCode.Text = order.OrderCode;
                        cmbStatus.Text = order.Status;
                        txtDeliveryAddress.Text = order.DeliveryAddress;
                        dpOrderDate.SelectedDate = order.OrderDate;
                        dpDeliveryDate.SelectedDate = order.DeliveryDate;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки данных заказа: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                SaveOrder();
            }
        }

        private bool ValidateData()
        {
            HideError();

            if (string.IsNullOrWhiteSpace(txtOrderCode.Text))
            {
                ShowError("Артикул заказа обязателен для заполнения");
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmbStatus.Text))
            {
                ShowError("Необходимо выбрать статус заказа");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDeliveryAddress.Text))
            {
                ShowError("Адрес пункта выдачи обязателен для заполнения");
                return false;
            }

            if (dpOrderDate.SelectedDate == null)
            {
                ShowError("Необходимо указать дату заказа");
                return false;
            }

            return true;
        }

        private void SaveOrder()
        {
            try
            {
                using (var context = new Entities())
                {
                    Order order;

                    if (_isEditMode)
                    {
                        order = context.Orders.Find(_orderId);
                        if (order == null)
                        {
                            ShowError("Заказ не найден");
                            return;
                        }
                    }
                    else
                    {
                        order = new Order();
                        context.Orders.Add(order);
                    }

                    order.OrderCode = txtOrderCode.Text.Trim();
                    order.Status = cmbStatus.Text;
                    order.DeliveryAddress = txtDeliveryAddress.Text.Trim();
                    order.OrderDate = dpOrderDate.SelectedDate.Value;
                    order.DeliveryDate = dpDeliveryDate.SelectedDate;

                    context.SaveChanges();

                    MessageBox.Show(_isEditMode ? "Заказ успешно обновлен" : "Заказ успешно добавлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка сохранения заказа: {ex.Message}");
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