using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Windows.Media;
using System;

namespace KG_05
{
    public partial class MainWindow : Window
    {
        private BitmapSource _originalBitmap; // Оригинальное изображение
        private BitmapSource _currentBitmap;   // Текущее изменённое изображение

        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов интерфейса
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            // Загрузка изображения через диалоговое окно
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                _originalBitmap = new BitmapImage(new Uri(dialog.FileName));
                _currentBitmap = _originalBitmap; // Начинаем с оригинала
                imageDisplay.Source = _currentBitmap;

                InitializeHistogram(); // Инициализация гистограммы
                UpdateHistogram();     // Обновление гистограммы
            }
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            // Инвертирование цветов изображения
            if (_originalBitmap != null)
            {
                _currentBitmap = InvertColors(_currentBitmap);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource InvertColors(BitmapSource bitmap)
        {
            // Инвертирование цветов пикселей
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Процесс инвертирования
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = (byte)(255 - pixelData[i]);         // Blue
                pixelData[i + 1] = (byte)(255 - pixelData[i + 1]); // Green
                pixelData[i + 2] = (byte)(255 - pixelData[i + 2]); // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette, pixelData, stride);
        }

        private void ConvertToGrayscale_Click(object sender, RoutedEventArgs e)
        {
            // Конвертирование в оттенки серого
            if (_originalBitmap != null)
            {
                _currentBitmap = ConvertToGrayscale(_currentBitmap);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource ConvertToGrayscale(BitmapSource bitmap)
        {
            // Процесс конвертации в оттенки серого
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Процесс конвертации
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte gray = (byte)(0.299 * pixelData[i + 2] + 0.587 * pixelData[i + 1] + 0.114 * pixelData[i]);
                pixelData[i] = gray;         // Blue
                pixelData[i + 1] = gray;     // Green
                pixelData[i + 2] = gray;     // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null, pixelData, stride);
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Изменение яркости изображения
            if (_originalBitmap != null)
            {
                _currentBitmap = AdjustBrightness(_currentBitmap, (int)brightnessSlider.Value);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource AdjustBrightness(BitmapSource bitmap, double adjustment)
        {
            // Регулировка яркости изображения
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Применение изменений яркости
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = Clamp((byte)(pixelData[i] + adjustment));         // Blue
                pixelData[i + 1] = Clamp((byte)(pixelData[i + 1] + adjustment)); // Green
                pixelData[i + 2] = Clamp((byte)(pixelData[i + 2] + adjustment)); // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette, pixelData, stride);
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Изменение контрастности изображения
            if (_originalBitmap != null)
            {
                _currentBitmap = AdjustContrast(_currentBitmap, contrastSlider.Value);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource AdjustContrast(BitmapSource bitmap, double adjustment)
        {
            // Регулировка контрастности изображения
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            double averageGray = CalculateAverageBrightness(bitmap);
            double factor = (259 * (adjustment + 255)) / (255 * (259 - adjustment));

            // Применение изменений контрастности
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = Clamp((byte)(factor * (pixelData[i] - averageGray) + averageGray));         // Blue
                pixelData[i + 1] = Clamp((byte)(factor * (pixelData[i + 1] - averageGray) + averageGray)); // Green
                pixelData[i + 2] = Clamp((byte)(factor * (pixelData[i + 2] - averageGray) + averageGray)); // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, bitmap.Format, bitmap.Palette, pixelData, stride);
        }

        private double CalculateAverageBrightness(BitmapSource bitmap)
        {
            // Расчёт средней яркости изображения
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            double totalBrightness = 0;

            // Суммирование яркости
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                totalBrightness += (.299 * pixelData[i + 2] + .587 * pixelData[i + 1] + .114 * pixelData[i]);
            }

            return totalBrightness / (width * height); // Возврат средней яркости
        }

        private void BinaryButton_Click(object sender, RoutedEventArgs e)
        {
            // Применение бинаризации изображения
            if (_originalBitmap != null)
            {
                byte threshold = (byte)thresholdSlider.Value;
                _currentBitmap = Binarization(_currentBitmap, threshold);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource Binarization(BitmapSource bitmap, byte threshold)
        {
            // Бинаризация изображения
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Применение бинаризации к пикселям
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte gray = (byte)(0.299 * pixelData[i + 2] + 0.587 * pixelData[i + 1] + 0.114 * pixelData[i]);
                byte binaryValue = gray < threshold ? (byte)0 : (byte)255;
                pixelData[i] = binaryValue;         // Blue
                pixelData[i + 1] = binaryValue;     // Green
                pixelData[i + 2] = binaryValue;     // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null, pixelData, stride);
        }

        private void UpdateHistogram()
        {
            // Обновление гистограммы
            if (_currentBitmap != null)
            {
                int[] histogram = CalculateHistogram(_currentBitmap); // Получение гистограммы

                double[] values = new double[256];
                for (int i = 0; i < histogram.Length; i++)
                {
                    values[i] = Convert.ToDouble(histogram[i]); // Конвертация в массив значений
                }

                var series = new ColumnSeries // Создание серии для графика
                {
                    Title = "Image Histogram",
                    Values = new ChartValues<double>(values),
                    Width = 0.1,
                    ColumnPadding = 0,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                if (Histogram.Series.Count > 0)
                    Histogram.Series.Clear(); // Очистка старых данных

                Histogram.Series.Add(series); // Добавление новой серии
                Histogram.AxisY[0].MinValue = 0; // Минимальное значение по оси Y
                Histogram.Update(true); // Обновление графика
            }
        }

        private int[] CalculateHistogram(BitmapSource bitmap)
        {
            // Вычисление гистограммы
            int[] histogram = new int[256];
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Заполнение массива гистограммы
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte brightness = (byte)((.299 * pixelData[i + 2]) + (.587 * pixelData[i + 1]) + (.114 * pixelData[i]));
                histogram[brightness]++;
            }

            return histogram; // Возврат массива гистограммы
        }

        private void InitializeHistogram()
        {
            // Инициализация гистограммы
            if (Histogram.Series.Count == 0)
            {
                // Создание осей для графика
                Histogram.AxisX.Clear();
                Histogram.AxisX.Add(new Axis
                {
                    Title = "Brightness",
                    MinValue = 0,
                    Separator = new Separator() // Разделитель осей
                });

                Histogram.AxisY.Clear();
                Histogram.AxisY.Add(new Axis
                {
                    Title = "Frequency",
                    MinValue = 0,
                    Separator = new Separator()
                });
            }
        }

        private byte Clamp(byte value)
        {
            // Ограничение значения в пределах 0-255
            return (byte)Math.Max(0, Math.Min(255, (int)value));
        }

        private void ConvertToBinary_Click(object sender, RoutedEventArgs e)
        {
            // Конвертация в бинарное изображение
            if (_originalBitmap != null)
            {
                byte threshold = (byte)thresholdSlider.Value;
                _currentBitmap = ConvertToBinary(_currentBitmap, threshold);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private BitmapSource ConvertToBinary(BitmapSource bitmap, byte threshold)
        {
            // Бинаризация изображения с использованием порога
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Создание нового массива для бинарных пикселей
            byte[] binaryData = new byte[height * stride];

            // Применение бинаризации
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte gray = (byte)(0.299 * pixelData[i + 2] + 0.587 * pixelData[i + 1] + 0.114 * pixelData[i]);
                byte binaryValue = gray < threshold ? (byte)0 : (byte)255;
                binaryData[i] = binaryValue;         // Blue
                binaryData[i + 1] = binaryValue;     // Green
                binaryData[i + 2] = binaryValue;     // Red
                binaryData[i + 3] = 255;              // Alpha
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null, binaryData, stride);
        }

        private BitmapSource GetNegative(BitmapSource bitmap)
        {
            // Получение негативного изображения
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            // Процесс создания негативного изображения
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = (byte)(255 - pixelData[i]);         // Blue
                pixelData[i + 1] = (byte)(255 - pixelData[i + 1]); // Green
                pixelData[i + 2] = (byte)(255 - pixelData[i + 2]); // Red
            }

            return BitmapSource.Create(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null, pixelData, stride);
        }

        private void GetNegative_Click(object sender, RoutedEventArgs e)
        {
            // Генерация негативного изображения
            if (_originalBitmap != null)
            {
                _currentBitmap = GetNegative(_currentBitmap);
                imageDisplay.Source = _currentBitmap;
                UpdateHistogram();
            }
        }

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {
            // Сброс изображения до оригинала
            if (_originalBitmap != null)
            {
                _currentBitmap = _originalBitmap; // Возвращаемся к оригиналу
                imageDisplay.Source = _currentBitmap;
                brightnessSlider.Value = 0; // Сбрасываем яркость
                contrastSlider.Value = 0;   // Сбрасываем контрастность
                UpdateHistogram(); // Обновляем гистограмму
            }
        }
    }
}
