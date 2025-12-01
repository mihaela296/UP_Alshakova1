using ShoeStore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace UP_Alshakova
{
    public partial class ClientWindow : Window
    {
        private string _userName;
        private int _userId;

        public ClientWindow(string userName, int userId = 0)
        {
            InitializeComponent();
            _userName = userName;
            _userId = userId;
            txtUserInfoHeader.Text = _userName;
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new Entities())
                {
                    var products = context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Supplier)
                        .Include(p => p.Unit)
                        .ToList();

                    var productList = products.Select(p => new
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Description = p.Description,
                        Manufacturer = p.Manufacturer.ManufacturerName,
                        Supplier = p.Supplier.SupplierName,
                        Price = p.Price,
                        FinalPrice = Math.Round(p.Price * (1 - ((p.Discount ?? 0) / 100)), 2),
                        UnitName = p.Unit.UnitName,
                        StockQuantity = p.StockQuantity,
                        Discount = p.Discount ?? 0,
                        HasDiscount = (p.Discount ?? 0) > 0,
                        ImagePath = string.IsNullOrEmpty(p.ImagePath) ?
                                   "Images/picture.png" : p.ImagePath,
                        BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity)
                    }).ToList();

                    itemsProducts.ItemsSource = productList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private SolidColorBrush GetBackgroundColor(decimal discount, int stockQuantity)
        {
            if (stockQuantity == 0)
                return (SolidColorBrush)FindResource("OutOfStockBrush");
            if (discount > 15)
                return (SolidColorBrush)FindResource("HighDiscountBrush");

            return (SolidColorBrush)FindResource("PrimaryBackgroundBrush");
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_userId > 0)
            {
                UserProfileWindow profileWindow = new UserProfileWindow(_userId);
                profileWindow.ShowDialog();
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_userId > 0)
            {
                ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(_userId);
                changePasswordWindow.ShowDialog();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}