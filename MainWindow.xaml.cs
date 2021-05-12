using ImgBytes;
using ImgConvert;
using Import;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCOI_4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IImgBytes img;
        int ThreadUse = 1;
        bool _flagFirstStart = true;
        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += MainWindow_SizeChanged;
            img = new ImgMatrix(new BitmapImage(new Uri("C:\\Users\\Vlad\\Downloads\\леена.jpg", UriKind.RelativeOrAbsolute)));

            SliderImage.Delay = 50;
            SliderImage.ValueChanged += SliderImage_ValueChanged;
            SliderImage.MouseLeftButtonUp += SliderImage_MouseLeftButtonUp;

            ThreadSlider.Maximum = ThreadCPP.ThreadCount();
            ThreadSlider.ValueChanged += ThreadSlider_ValueChanged;

            Apply.Checked += Apply_Checked;
            Apply.Unchecked += Apply_Unchecked;
        }

        private void SliderImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ThreadSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThreadUse = (int)e.NewValue;
            ThreadValue.Text = ((int)e.NewValue).ToString();
        }

        DateTime LastUpd = DateTime.UtcNow;
        int Delay = 50;
        private void SliderImage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Apply.IsChecked.Value)
            {
                var TimeFromLastUpd = DateTime.UtcNow - LastUpd;
                if ((int)e.NewValue == 100)
                {
                    LastUpd = DateTime.UtcNow;
                    UseMatrixWithoutMessage();
                    return;
                }
                if ((int)e.NewValue == 0)
                {
                    LastUpd = DateTime.UtcNow;
                    UseMatrixWithoutMessage();
                    return;
                }
                //if (TimeFromLastUpd.TotalMilliseconds > Delay)
                {
                    LastUpd = DateTime.UtcNow;
                    UseMatrixWithoutMessage();
                    Delay = (int)TimeFromLastUpd.TotalMilliseconds;
                }
            }
        }

        private void Apply_Checked(object sender, RoutedEventArgs e)
        {
            UseMatrix();
        }
        private void Apply_Unchecked(object sender, RoutedEventArgs e)
        {
            Picture.Source = img.BitmapSourseOrig;
        }

        //Scale window
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_flagFirstStart)
            {
                var deltaHeight = e.NewSize.Height - e.PreviousSize.Height;
                var deltaWidth = e.NewSize.Width - e.PreviousSize.Width;
                Picture.Height += deltaHeight;
                Picture.Width += deltaWidth;
                SliderImage.Height += deltaHeight;
                SliderImage.Margin = new Thickness(SliderImage.Margin.Left + deltaWidth, SliderImage.Margin.Top, 0, 0);
                NavBar.Width += deltaWidth;
                Log.Margin = new Thickness(Log.Margin.Left + deltaWidth, Log.Margin.Top, 0, 0);
                Log.Height += deltaHeight;
                InputMatrix.Margin = new Thickness(InputMatrix.Margin.Left, InputMatrix.Margin.Top + deltaHeight, 0, 0);
                InputMatrix.Width += deltaWidth;
            }
            else
            {
                _flagFirstStart = false;
            }
        }

        void UseMatrix()
        {
            var start = DateTime.UtcNow;
            MyMatrix matrix = new MyMatrix(InputMatrix.Text);
            if (matrix.Matrix == null)
            {
                WriteLog("Матрица введена не корректно", Brushes.Red);
                return;
            }
            if (!matrix.IsOddNumber)
            {
                WriteLog("Матрица не прямоугольная или не нечетная", Brushes.Red);
                return;
            }
            Picture.Source = ((ImgMatrix)img).ApplyMatrix(matrix, ThreadUse, 100 - (int)SliderImage.Value);
            WriteLog("Загружено " + (DateTime.UtcNow - start).TotalMilliseconds, Brushes.DarkGreen);
        }

        void UseMatrixWithoutMessage()
        {
            MyMatrix matrix = new MyMatrix(InputMatrix.Text);
            if (matrix.Matrix == null)
            {
                return;
            }
            if (!matrix.IsOddNumber)
            {
                return;
            }
            Picture.Source = ((ImgMatrix)img).ApplyMatrix(matrix, ThreadUse,100 - (int)SliderImage.Value);
        }


        //Кнопочки
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            OpenFileDialog fileManager = new OpenFileDialog();
            fileManager.Filter = "Файлы jpg|*.jpg|Файлы jpeg|*.jpeg|Файлы png| *.png";
            fileManager.ShowDialog();
            var item = fileManager.FileName;
            if (item != "")
            {
                img = new ImgMatrix(new BitmapImage(new Uri(item, UriKind.RelativeOrAbsolute)));
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                UseMatrix();
                WriteLog("Загружено", Brushes.DarkGreen);
            }
        }

        private void FileIsDropped(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData("FileDrop");
            try
            {
                foreach (var item in paths)
                {
                    var start = DateTime.UtcNow;

                    img = new ImgMatrix(new BitmapImage(new Uri(item, UriKind.RelativeOrAbsolute)));
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    UseMatrix();
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, Brushes.Red);
            }
        }


        private void SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileManager = new SaveFileDialog();
            fileManager.Filter = "Файлы jpg|*.jpg|Файлы jpeg|*.jpeg|Файлы png| *.png";
            fileManager.ShowDialog();
            var item = fileManager.FileName;
            try
            {
                if (item != "")
                {
                    Picture.Source.Save(item);
                }
                WriteLog("Файл " + item + " успешно сохранен", Brushes.DarkBlue);
            }
            catch
            {
                WriteLog("Не удалось сохранить в указанный файл", Brushes.Red);
            }
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(Picture.Source as BitmapSource);
            WriteLog("Скопировано", Brushes.DarkOrange);
        }

        private void CutClick(object sender, RoutedEventArgs e)
        {

        }

        private void MatrixGenerate(object sender, RoutedEventArgs e)
        {
            try
            {
                int r = int.Parse(SizeValue.Text);
                double sigm = double.Parse(SigmaValue.Text);
                MyMatrix matrix = new MyMatrix(r, sigm);
                InputMatrix.Text = matrix.ToString();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, Brushes.Red);
            }
        }

        private void MedianClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var start = DateTime.UtcNow;
                int r = int.Parse(MedianValue.Text);
                Picture.Source = ((ImgMatrix)img).MedianFilter(r, (int)ThreadSlider.Value);
                WriteLog("Загружено " + (DateTime.UtcNow - start).TotalMilliseconds, Brushes.DarkGreen);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, Brushes.Red);
            }
        }

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                img = new ImgMatrix(Clipboard.GetImage());
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                UseMatrix();
            }
            else
            {
                WriteLog("Не картинка", Brushes.Red);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void WriteLog(string message, System.Windows.Media.SolidColorBrush color = null)
        {
            if (color == null)
                color = System.Windows.Media.Brushes.Black;
            var text = new TextBlock() { Text = message, Foreground = color };
            Log.Items.Add(text);
            Log.ScrollIntoView(text);
            Log.SelectedItem = text;
        }

        private void MemoryLog(object sender, RoutedEventArgs e)
        {
            //Показать память
            Process process = Process.GetCurrentProcess();
            long memoryAmount = process.WorkingSet64;
            WriteLog("Памяти скушано - " + (memoryAmount / (1024 * 1024)).ToString(), Brushes.Purple);
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            Log.Items.Clear();
        }
    }
}
