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
    public partial class AdminWindow : Window
    {
        private string _userName;
        private int _userId;
        private ObservableCollection<dynamic> _allProducts;
        private ObservableCollection<dynamic> _filteredProducts;

        public AdminWindow(string userName, int userId = 0)
        {
            InitializeComponent();
            _userName = userName;
            _userId = userId;
            txtUserInfo.Text = _userName;
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
                            FinalPrice = p.Price * (1 - ((p.Discount ?? 0) / 100)),
                            UnitName = p.Unit.UnitName,
                            StockQuantity = p.StockQuantity,
                            Discount = p.Discount ?? 0,
                            ImagePath = string.IsNullOrEmpty(p.ImagePath) ?
                                       "Images/picture.png" : p.ImagePath,
                            BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity)
                        }).ToList()
                    );

                    _filteredProducts = new ObservableCollection<dynamic>(_allProducts);
                    dgProducts.ItemsSource = _filteredProducts;
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
            OrdersWindow ordersWindow = new OrdersWindow("Admin", _userName);
            ordersWindow.Show();
            this.Close();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductEditWindow editWindow = new ProductEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
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

        private void dgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem != null)
            {
                dynamic selectedProduct = dgProducts.SelectedItem;
                ProductEditWindow editWindow = new ProductEditWindow(selectedProduct.ProductID);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext != null)
            {
                dynamic product = button.DataContext;
                int productId = product.ProductID;

                if (MessageBox.Show($"Вы уверены, что хотите удалить товар '{product.ProductName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DeleteProduct(productId);
                }
            }
        }

        private void DeleteProduct(int productId)
        {
            try
            {
                using (var context = new Entities())
                {
                    var product = context.Products.Find(productId);
                    if (product != null)
                    {
                        context.Products.Remove(product);
                        context.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Товар успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            // Сортировка
            if (cmbSort.SelectedIndex == 1) // Количество ↑
                filtered = filtered.OrderBy(p => p.StockQuantity);
            else if (cmbSort.SelectedIndex == 2) // Количество ↓
                filtered = filtered.OrderByDescending(p => p.StockQuantity);
            else if (cmbSort.SelectedIndex == 3) // Цена ↑
                filtered = filtered.OrderBy(p => p.Price);
            else if (cmbSort.SelectedIndex == 4) // Цена ↓
                filtered = filtered.OrderByDescending(p => p.Price);

            _filteredProducts.Clear();
            foreach (var product in filtered)
            {
                _filteredProducts.Add(product);
            }
        }
    }
}