using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Tubes3_SakedikKasep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        public string algoritma;
        public string img;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void uploadImage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".jpg";
                dlg.Filter = "Image Files |*.bmp;*.jpg;*.png";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    string filename = dlg.FileName;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filename);
                    bitmap.EndInit();
                    imgUpld.Source = bitmap;
                    Console.WriteLine(bitmap);
                    img = filename;
                    fingerImg.Visibility = Visibility.Collapsed;
                    txtFinger.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void closeWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);    
            }
        }

        private void selectAlgo(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = sender as Button;

                if (clickedButton == KMPButton)
                {
                    algoritma = "KMP";
                    KMPButton.Background = Brushes.LightGray;
                    KMPButton.Foreground = Brushes.IndianRed;
                    BMButton.Background = Brushes.DarkCyan;
                    BMButton.Foreground = Brushes.White;
                }
                else if (clickedButton == BMButton)
                {
                    algoritma = "BM";
                    BMButton.Background = Brushes.LightGray;
                    BMButton.Foreground = Brushes.DarkCyan;
                    KMPButton.Background = Brushes.IndianRed;
                    KMPButton.Foreground = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void deleteImg(object sender, MouseButtonEventArgs e)
        {
            try
            {
                imgUpld.Source = null;
                fingerImg.Visibility = Visibility.Visible;
                txtFinger.Visibility = Visibility.Visible;
                img = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}