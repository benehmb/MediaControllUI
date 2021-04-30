using System;
using System.Globalization;
using System.Windows.Data;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;
using System.IO;

namespace MediaControll
{
    /// <summary>
    /// Converts a <see cref="byte[]"/> to a <see cref="BitmapImage"/>
    /// </summary>
    [System.Windows.Data.ValueConversion(typeof(byte[]), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public static ImageConverter Instance = new ImageConverter();


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is byte[]))
                return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream((byte[])value))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}