using ShoeStore;
using System.Collections.ObjectModel;
using System.Data.Entity; // для Entity Framework
using System.Linq;
using System.Windows;

namespace UP_Alshakova
{
    public partial class GuestWindow : Window
    {
        public GuestWindow()
        {
            InitializeComponent();
            LoadProducts();
            txtUserInfo.Text = "Гость";
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Загружаем товары из БД через Entity Framework
                    var products = context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Supplier)
                        .Include(p => p.Unit)
                        .ToList();

                    // Преобразуем в коллекцию для отображения
                    var productList = products.Select(p => new
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
                        StockQuantity = p.StockQuantity, // Просто значение, без ??
                        Discount = p.Discount ?? 0,
                        ImagePath = string.IsNullOrEmpty(p.ImagePath) ?
                                   "Images/picture.png" : p.ImagePath,
                        BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity), // Без ?? для StockQuantity
                        PriceDecoration = (p.Discount ?? 0) > 0 ?
                                        TextDecorations.Strikethrough : null,
                        FinalPriceVisibility = (p.Discount ?? 0) > 0 ?
                                             Visibility.Visible : Visibility.Collapsed
                    }).ToList();

                    itemsProducts.ItemsSource = productList;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Media.SolidColorBrush GetBackgroundColor(decimal discount, int stockQuantity)
        {
            if (stockQuantity == 0)
                return (System.Windows.Media.SolidColorBrush)FindResource("OutOfStockBrush");
            if (discount > 15)
                return (System.Windows.Media.SolidColorBrush)FindResource("HighDiscountBrush");

            return (System.Windows.Media.SolidColorBrush)FindResource("PrimaryBackgroundBrush");
        }
    }
}