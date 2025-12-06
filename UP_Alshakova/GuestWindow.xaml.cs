using ShoeStore;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace UP_Alshakova
{
    public partial class GuestWindow : Window
    {
        private ObservableCollection<ProductViewModel> _products;

        public GuestWindow()
        {
            InitializeComponent();
            _products = new ObservableCollection<ProductViewModel>();
            itemsProducts.ItemsSource = _products;

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

                    _products.Clear();

                    foreach (var p in products)
                    {
                        var productVM = new ProductViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName ?? "Без названия",
                            CategoryName = p.Category?.CategoryName ?? "Не указано",
                            Description = p.Description ?? "Описание отсутствует",
                            Manufacturer = p.Manufacturer?.ManufacturerName ?? "Не указано",
                            Supplier = p.Supplier?.SupplierName ?? "Не указано",
                            Price = p.Price,
                            FinalPrice = System.Math.Round(p.Price * (1 - ((p.Discount ?? 0) / 100)), 2),
                            UnitName = p.Unit?.UnitName ?? "шт.",
                            StockQuantity = p.StockQuantity,
                            Discount = p.Discount ?? 0,
                            HasDiscount = (p.Discount ?? 0) > 0,
                            ImagePath = p.ImagePath, // Будет автоматически загружено изображение
                            BackgroundColor = GetBackgroundColor(p.Discount ?? 0, p.StockQuantity)
                        };

                        _products.Add(productVM);
                    }
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