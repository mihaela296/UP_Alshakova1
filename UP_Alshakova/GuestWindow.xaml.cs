using ShoeStore;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
            txtUserInfoHeader.Text = "Гость";
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
                        FinalPrice = System.Math.Round(p.Price * (1 - ((p.Discount ?? 0) / 100)), 2),
                        UnitName = p.Unit.UnitName,
                        StockQuantity = p.StockQuantity,
                        Discount = p.Discount ?? 0,
                        ImagePath = string.IsNullOrEmpty(p.ImagePath) ?
                                   "Images/picture.png" : p.ImagePath,
                        BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity)
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