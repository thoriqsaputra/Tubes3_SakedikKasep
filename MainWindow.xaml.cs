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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using static AlayConverter;
using System.Data.SQLite;
using System.CodeDom.Compiler;
using System.IO;


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
            string connectionString = "Data Source=biodata.db;Version=3;";
            Dictionary<string, string> sidikJariMap = new Dictionary<string, string>();
            Dictionary<string, Dictionary<string, string>> dataMap = new Dictionary<string, Dictionary<string, string>>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // Membuka koneksi
                connection.Open();

                // Membuat command untuk mengambil data
                string query = "SELECT * FROM sidik_jari";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // Mengeksekusi query dan mendapatkan data
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Membaca data
                        while (reader.Read())
                        {
                            // Misalkan tabel memiliki kolom "id" (integer) dan "name" (string)
                            string id = reader.GetString(0); // Kolom pertama
                            string name = reader.GetString(1); // Kolom kedua

                            // Tambahkan ke dictionary
                            sidikJariMap[id] = name;
                        }
                    }
                }

                query = "SELECT * FROM biodata";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Nama sebagai kunci (key), sisanya sebagai nilai (value)
                            string nama = reader["nama"].ToString();

                            // Periksa apakah kunci sudah ada sebelum menambahkannya
                            if (!dataMap.ContainsKey(nama))
                            {
                                Dictionary<string, string> attributes = new Dictionary<string, string>
                            {
                                { "NIK", reader["NIK"].ToString() },
                                { "tempat_lahir", reader["tempat_lahir"].ToString() },
                                { "tanggal_lahir", reader["tanggal_lahir"].ToString() },
                                { "jenis_kelamin", reader["jenis_kelamin"].ToString() },
                                { "golongan_darah", reader["golongan_darah"].ToString() },
                                { "alamat", reader["alamat"].ToString() },
                                { "agama", reader["agama"].ToString() },
                                { "status_perkawinan", reader["status_perkawinan"].ToString() },
                                { "pekerjaan", reader["pekerjaan"].ToString() },
                                { "kewarganegaraan", reader["kewarganegaraan"].ToString() }
                            };
                                // Tambahkan ke dalam Dictionary dataMap
                                dataMap.Add(nama, attributes);
                            }
                        }
                    }
                }

                // "../img/1.BMP"
                Console.WriteLine("Selamat datang brooo");
                Console.Write("Silahkan masukkan path gambar yang ingin disamakan: ");
                string pathSearch = "img/1.BMP";
                Console.WriteLine("Pilihan algoritma: ");
                Console.WriteLine("1. KMP");
                Console.WriteLine("2. BM");
                Console.Write("Masukkan pilihan algoritma dengan angka: ");
                string algo = "1";


                Bitmap patternBMP = new Bitmap(pathSearch);
                patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 1);

                string folderPath = "img";
                string[] bmpFiles = Directory.GetFiles(folderPath, "*.bmp", SearchOption.TopDirectoryOnly);

                double maxSimilarity = 0;
                var path = "";
                foreach (var bmpPath in bmpFiles)
                {

                    if (algo == "1")
                    {
                        Bitmap bmp = new Bitmap(bmpPath);
                        string asciiArt = BitmapToBinaryAsciiConverter.ConvertToAscii(patternBMP);
                        string oir = BitmapToBinaryAsciiConverter.ConvertToAscii(bmp);
                        var result = KMPAlgorithm.KmpMatch(oir, asciiArt);
                        if (result.similarity == 1)
                        {
                            maxSimilarity = result.similarity;
                            path = bmpPath;
                            break;
                        }
                        if (result.similarity > maxSimilarity)
                        {
                            maxSimilarity = result.similarity;
                            path = bmpPath;
                        }
                    }
                    else
                    {
                        Bitmap bmp = new Bitmap(bmpPath);
                        string asciiArt = BitmapToBinaryAsciiConverter.ConvertToAscii(patternBMP);
                        string oir = BitmapToBinaryAsciiConverter.ConvertToAscii(bmp);
                        double similarity = BoyerMoore.BmMatch(oir, asciiArt);
                        if (similarity == 1)
                        {
                            maxSimilarity = similarity;
                            path = bmpPath;
                            break;
                        }
                        if (similarity > maxSimilarity)
                        {
                            maxSimilarity = similarity;
                            path = bmpPath;
                        }
                    }
                }
                Console.Write("path yang sama: ");
                Console.WriteLine(path);
                Console.Write("similarity: ");
                Console.WriteLine(maxSimilarity);
                string namaFile = System.IO.Path.GetFileNameWithoutExtension(path);

                if (sidikJariMap.TryGetValue(namaFile, out string value))
                {
                    Console.WriteLine($"Name for ID {namaFile}: {value}");
                    double tempMaxSim = 0;
                    string name = "";
                    foreach (var entry in dataMap)
                    {
                        string tempName = AlayConverter.RevertAlay(entry.Key);
                        var set1 = new HashSet<char>(value);
                        var set2 = new HashSet<char>(tempName);

                        int intersectionCount = set1.Intersect(set2).Count();
                        int unionCount = set1.Union(set2).Count();

                        double similarity = (double)intersectionCount / unionCount;

                        if (similarity > tempMaxSim)
                        {
                            tempMaxSim = similarity;
                            name = entry.Key;
                        }
                    }
                    Console.WriteLine(name);
                    if (dataMap.ContainsKey(name))
                    {
                        Dictionary<string, string> attribute = dataMap[name];
                        foreach (var pair in attribute)
                        {
                            Console.WriteLine($"{pair.Key}: {pair.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Key tidak ditemukan.");
                    }
                }
                else
                {
                    Console.WriteLine($"ID {namaFile} tidak ditemukan.");
                }
            }

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
                    KMPButton.Background = System.Windows.Media.Brushes.LightGray;
                    KMPButton.Foreground = System.Windows.Media.Brushes.IndianRed;
                    BMButton.Background = System.Windows.Media.Brushes.DarkCyan;
                    BMButton.Foreground = System.Windows.Media.Brushes.White;
                }
                else if (clickedButton == BMButton)
                {
                    algoritma = "BM";
                    BMButton.Background = System.Windows.Media.Brushes.LightGray;
                    BMButton.Foreground = System.Windows.Media.Brushes.DarkCyan;
                    KMPButton.Background = System.Windows.Media.Brushes.IndianRed;
                    KMPButton.Foreground = System.Windows.Media.Brushes.White;
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