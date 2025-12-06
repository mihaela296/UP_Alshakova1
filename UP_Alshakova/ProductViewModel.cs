using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UP_Alshakova
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string UnitName { get; set; }
        public int StockQuantity { get; set; }
        public decimal Discount { get; set; }
        public bool HasDiscount { get; set; }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                ProductImage = LoadImage(value);
            }
        }

        public ImageSource ProductImage { get; private set; }
        public SolidColorBrush BackgroundColor { get; set; }

        private ImageSource LoadImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return LoadDefaultImage();
                }

                // Пробуем разные варианты путей
                string[] possiblePaths =
                {
                    imagePath,
                    "Image/" + imagePath,
                    "image/" + imagePath,
                    "/Image/" + imagePath,
                    "/image/" + imagePath,
                    "pack://application:,,,/Image/" + imagePath
                };

                foreach (var path in possiblePaths)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        if (bitmap.PixelWidth > 0)
                            return bitmap;
                    }
                    catch { }
                }

                return LoadDefaultImage();
            }
            catch
            {
                return LoadDefaultImage();
            }
        }

        private ImageSource LoadDefaultImage()
        {
            try
            {
                // Пробуем разные пути к дефолтному изображению
                string[] defaultPaths =
                {
                    "Image/picture.png",
                    "image/picture.png",
                    "/Image/picture.png",
                    "/image/picture.png",
                    "pack://application:,,,/Image/picture.png"
                };

                foreach (var path in defaultPaths)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        if (bitmap.PixelWidth > 0)
                            return bitmap;
                    }
                    catch { }
                }
            }
            catch { }

            // Если совсем ничего не получилось
            return null;
        }
    }
}