using System.Windows;
using System.Data.Entity;
using System.Linq;

namespace UP_Alshakova
{
    public partial class ProductDetailsWindow : Window
    {
        private int _productId;

        public ProductDetailsWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductDetails();
        }

        private void LoadProductDetails()
        {
            try
            {
                using (var context = new Entities())
                {
                    var product = context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Supplier)
                        .Include(p => p.Unit)
                        .FirstOrDefault(p => p.ProductID == _productId);

                    if (product != null)
                    {
                        // Заголовок окна
                        Title = product.ProductName;

                        // Основная информация
                        txtProductName.Text = product.ProductName;
                        txtCategory.Text = $"Категория: {product.Category.CategoryName}";
                        txtDescription.Text = product.Description;
                        txtManufacturer.Text = $"Производитель: {product.Manufacturer.ManufacturerName}";
                        txtSupplier.Text = $"Поставщик: {product.Supplier.SupplierName}";
                        txtUnit.Text = $"Единица измерения: {product.Unit.UnitName}";
                        txtStock.Text = $"Количество на складе: {product.StockQuantity} шт.";
                        txtDiscount.Text = $"Скидка: {product.Discount ?? 0}%";

                        // Цены
                        decimal finalPrice = product.Price * (1 - ((product.Discount ?? 0) / 100));

                        if (product.Discount > 0)
                        {
                            txtOriginalPrice.Text = $"{product.Price:C}";
                            txtOriginalPrice.TextDecorations = System.Windows.TextDecorations.Strikethrough;
                            txtFinalPrice.Text = $"{finalPrice:C}";
                        }
                        else
                        {
                            txtOriginalPrice.Text = $"{product.Price:C}";
                            txtFinalPrice.Visibility = Visibility.Collapsed;
                        }

                        // Изображение
                        if (!string.IsNullOrEmpty(product.ImagePath) && System.IO.File.Exists(product.ImagePath))
                        {
                            imgProduct.Source = new System.Windows.Media.Imaging.BitmapImage(
                                new System.Uri(product.ImagePath));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Товар не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки информации о товаре: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}