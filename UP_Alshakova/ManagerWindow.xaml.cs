using ShoeStore;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UP_Alshakova
{
    public partial class ManagerWindow : Window
    {
        private string _userName;
        private int _userId;
        private ObservableCollection<dynamic> _allProducts;
        private ObservableCollection<dynamic> _filteredProducts;

        public ManagerWindow(string userName, int userId = 0)
        {
            InitializeComponent();
            _userName = userName;
            _userId = userId;
            txtUserInfoHeader.Text = _userName;
            LoadProducts();
            LoadFilters();
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

                    _allProducts = new ObservableCollection<dynamic>(
                        products.Select(p => new
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
                            ImagePath = string.IsNullOrEmpty(p.ImagePath) ?
                                       "Images/picture.png" : p.ImagePath,
                            BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity)
                        }).ToList()
                    );

                    _filteredProducts = new ObservableCollection<dynamic>(_allProducts);
                    itemsProducts.ItemsSource = _filteredProducts;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilters()
        {
            // Загрузка поставщиков
            cmbSupplier.Items.Clear();
            cmbSupplier.Items.Add("Все поставщики");

            using (var context = new Entities())
            {
                var suppliers = context.Suppliers.Select(s => s.SupplierName).Distinct().ToList();
                foreach (var supplier in suppliers)
                {
                    cmbSupplier.Items.Add(supplier);
                }
            }
            cmbSupplier.SelectedIndex = 0;
        }

        private SolidColorBrush GetBackgroundColor(decimal discount, int stockQuantity)
        {
            if (stockQuantity == 0)
                return (SolidColorBrush)FindResource("OutOfStockBrush");
            if (discount > 15)
                return (SolidColorBrush)FindResource("HighDiscountBrush");

            return (SolidColorBrush)FindResource("PrimaryBackgroundBrush");
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWindow = new OrdersWindow("Manager", _userName);
            ordersWindow.Show();
            this.Close();
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_userId > 0)
            {
                UserProfileWindow profileWindow = new UserProfileWindow(_userId);
                profileWindow.ShowDialog();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // Методы фильтрации и поиска
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allProducts == null) return;

            var filtered = _allProducts.AsEnumerable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    p.CategoryName.ToLower().Contains(searchText) ||
                    p.Description.ToLower().Contains(searchText) ||
                    p.Manufacturer.ToLower().Contains(searchText) ||
                    p.Supplier.ToLower().Contains(searchText));
            }

            // Фильтрация по поставщику
            if (cmbSupplier.SelectedItem != null && cmbSupplier.SelectedIndex > 0)
            {
                string selectedSupplier = cmbSupplier.SelectedItem.ToString();
                filtered = filtered.Where(p => p.Supplier == selectedSupplier);
            }

            // Сортировка (только по количеству)
            if (cmbSort.SelectedIndex == 1) // Количество ↑
                filtered = filtered.OrderBy(p => p.StockQuantity);
            else if (cmbSort.SelectedIndex == 2) // Количество ↓
                filtered = filtered.OrderByDescending(p => p.StockQuantity);

            _filteredProducts.Clear();
            foreach (var product in filtered)
            {
                _filteredProducts.Add(product);
            }
        }
    }
}