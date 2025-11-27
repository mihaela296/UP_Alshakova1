using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UP_Alshakova
{
    public partial class ProductEditWindow : Window
    {
        private int _productId;
        private bool _isEditMode;
        private string _currentImagePath;

        public ProductEditWindow(int productId = 0)
        {
            InitializeComponent();
            _productId = productId;
            _isEditMode = productId > 0;

            InitializeWindow();
            LoadComboBoxData();

            if (_isEditMode)
            {
                LoadProductData();
            }
        }

        private void InitializeWindow()
        {
            if (_isEditMode)
            {
                Title = "Редактирование товара";
                pnlProductID.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Добавление товара";
                pnlProductID.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadComboBoxData()
        {
            using (var context = new Entities())
            {
                // Загрузка категорий
                var categories = context.Categories.ToList();
                cmbCategory.ItemsSource = categories;
                cmbCategory.DisplayMemberPath = "CategoryName";
                cmbCategory.SelectedValuePath = "CategoryID";

                // Загрузка производителей
                var manufacturers = context.Manufacturers.ToList();
                cmbManufacturer.ItemsSource = manufacturers;
                cmbManufacturer.DisplayMemberPath = "ManufacturerName";
                cmbManufacturer.SelectedValuePath = "ManufacturerID";

                // Загрузка поставщиков
                var suppliers = context.Suppliers.ToList();
                cmbSupplier.ItemsSource = suppliers;
                cmbSupplier.DisplayMemberPath = "SupplierName";
                cmbSupplier.SelectedValuePath = "SupplierID";

                // Загрузка единиц измерения
                var units = context.Units.ToList();
                cmbUnit.ItemsSource = units;
                cmbUnit.DisplayMemberPath = "UnitName";
                cmbUnit.SelectedValuePath = "UnitID";
            }
        }

        private void LoadProductData()
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
                        txtProductID.Text = product.ProductID.ToString();
                        txtProductName.Text = product.ProductName;
                        cmbCategory.SelectedValue = product.CategoryID;
                        txtDescription.Text = product.Description;
                        cmbManufacturer.SelectedValue = product.ManufacturerID;
                        cmbSupplier.SelectedValue = product.SupplierID;
                        txtPrice.Text = product.Price.ToString();
                        cmbUnit.SelectedValue = product.UnitID;
                        txtStockQuantity.Text = product.StockQuantity.ToString();
                        txtDiscount.Text = (product.Discount ?? 0).ToString();

                        // Загрузка изображения
                        if (!string.IsNullOrEmpty(product.ImagePath) && File.Exists(product.ImagePath))
                        {
                            _currentImagePath = product.ImagePath;
                            imgProduct.Source = new BitmapImage(new Uri(product.ImagePath));
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки данных товара: {ex.Message}");
            }
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png",
                    Title = "Выберите изображение товара"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourcePath = openFileDialog.FileName;

                    // Создаем папку для изображений если не существует
                    string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "ProductImages");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Генерируем уникальное имя файла
                    string fileName = $"product_{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(sourcePath)}";
                    string destinationPath = Path.Combine(imagesFolder, fileName);

                    // Копируем файл
                    File.Copy(sourcePath, destinationPath, true);

                    // Удаляем старое изображение если оно было
                    if (!string.IsNullOrEmpty(_currentImagePath) && File.Exists(_currentImagePath))
                    {
                        File.Delete(_currentImagePath);
                    }

                    _currentImagePath = destinationPath;
                    imgProduct.Source = new BitmapImage(new Uri(destinationPath));
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private void btnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentImagePath) && File.Exists(_currentImagePath))
            {
                File.Delete(_currentImagePath);
            }

            _currentImagePath = null;
            imgProduct.Source = new BitmapImage(new Uri("Images/picture.png", UriKind.Relative));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                SaveProduct();
            }
        }

        private bool ValidateData()
        {
            // Сбрасываем ошибки
            HideError();

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                ShowError("Наименование товара обязательно для заполнения");
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                ShowError("Необходимо выбрать категорию");
                return false;
            }

            if (cmbManufacturer.SelectedItem == null)
            {
                ShowError("Необходимо выбрать производителя");
                return false;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                ShowError("Необходимо выбрать поставщика");
                return false;
            }

            // Проверка числовых полей
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                ShowError("Цена должна быть положительным числом");
                return false;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stockQuantity) || stockQuantity < 0)
            {
                ShowError("Количество на складе должно быть неотрицательным числом");
                return false;
            }

            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                ShowError("Скидка должна быть числом от 0 до 100");
                return false;
            }

            return true;
        }

        private void SaveProduct()
        {
            try
            {
                using (var context = new Entities())
                {
                    Product product;

                    if (_isEditMode)
                    {
                        // Редактирование существующего товара
                        product = context.Products.Find(_productId);
                        if (product == null)
                        {
                            ShowError("Товар не найден");
                            return;
                        }
                    }
                    else
                    {
                        // Добавление нового товара
                        product = new Product();
                        context.Products.Add(product);
                    }

                    // Заполняем данные
                    product.ProductName = txtProductName.Text.Trim();
                    product.CategoryID = (int)cmbCategory.SelectedValue;
                    product.Description = txtDescription.Text.Trim();
                    product.ManufacturerID = (int)cmbManufacturer.SelectedValue;
                    product.SupplierID = (int)cmbSupplier.SelectedValue;
                    product.Price = decimal.Parse(txtPrice.Text);
                    product.UnitID = (int)cmbUnit.SelectedValue;
                    product.StockQuantity = int.Parse(txtStockQuantity.Text);
                    product.Discount = decimal.Parse(txtDiscount.Text);
                    product.ImagePath = _currentImagePath;

                    context.SaveChanges();

                    MessageBox.Show(_isEditMode ? "Товар успешно обновлен" : "Товар успешно добавлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка сохранения товара: {ex.Message}");
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