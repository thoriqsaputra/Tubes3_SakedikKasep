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
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Linq;
using System.Data.Entity.Core;


namespace Tubes3_SakedikKasep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        public string algoritma;
        public Bitmap img;
        private Dictionary<string, string> sidikJariMap;
        private Dictionary<string, Dictionary<string, string>> dataMap;
        private String resultPath = "";
        private double similariti;
        private Dictionary<string, string> attribute;

        private string naem;

        public MainWindow()
        {
            InitializeComponent();

            string connectionString = "Data Source=biodata.db;Version=3;";
            sidikJariMap = new Dictionary<string, string>();
            dataMap = new Dictionary<string, Dictionary<string, string>>();
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



                
            }

        }

        private async void runAlgoritma(Object sender, RoutedEventArgs eventArgs)
        {
            // Cek if the image and algorithm has been selected or not
            if (img == null)
            {
                MessageBox.Show("Silahkan upload gambar sidik jari terlebih dahulu.");
                return;
            }
            else if (algoritma == null)
            {
                MessageBox.Show("Silahkan pilih algoritma terlebih dahulu.");
                return;
            }



            // show loading animation
            loading.Visibility = Visibility.Visible;

            Stopwatch stopwatch = new Stopwatch(); // create new stopwatch

            stopwatch.Start(); // start the stopwatch

            await Task.Run(() => Algoritma()); // Run the algorithm

            stopwatch.Stop(); // stop the stopwatch

            // get the elapsed time
            long elapsed_time = stopwatch.ElapsedMilliseconds;

            // hide loading animation
            loading.Visibility = Visibility.Collapsed;

            // Hide the placeHolder for the result image
            textMatch.Visibility = Visibility.Collapsed;
            matchP.Visibility = Visibility.Collapsed;

           
            // LATER ADD BOOLEAN WHEN KNOW WHERE TO PLACE IT AT
            if (resultPath == "") // REMBER TO ADD SOME BOOLEAN EXPRESSION WETHER THE IMAGE IS FOUND OR NOT
            {
                textMatch.Text = "Not Match";
                textMatch.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }
            else
            {
                // set the bio of the most similar fingerprint
                setBio(attribute);
                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

                // Combine the project directory with the relative path
                string absolutePath = System.IO.Path.Combine(projectDirectory, resultPath);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(absolutePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                // set the image to the result image
                IMGresult.Source = bitmap;
                // show the result image
                IMGresult.Visibility = Visibility.Visible;
                // set the text to match
                textMatch.Text = "Match";
                textMatch.Foreground = System.Windows.Media.Brushes.Green;

            }
            textMatch.Visibility = Visibility.Visible;
            // set the time taken to the text

            if (elapsed_time < 1000)
            {
                TimeTaken.Text = $"{elapsed_time} ms";
            }
            else if (elapsed_time < 60000)
            {
                TimeTaken.Text = $"{elapsed_time / 1000} s";
            }
            else
            {
                TimeTaken.Text = $"{elapsed_time / 60000} m";
            }

            // set the similarity to the text
            persen.Text = $"{similariti * 100}%";


        }

        public void setBio(Dictionary<string, string> attributes)
        {
            NIK.Text = attributes["NIK"];
            nama.Text = naem;
            tempatLahir.Text = attributes["tempat_lahir"];
            tanggalLahir.Text = attributes["tanggal_lahir"];
            jenisKelamin.Text = attributes["jenis_kelamin"];
            golonganDarah.Text = attributes["golongan_darah"];
            alamat.Text = attributes["alamat"];
            agama.Text = attributes["agama"];
            statusPerkawinan.Text = attributes["status_perkawinan"];
            pekerjaan.Text = attributes["pekerjaan"];
            kewarganegaraan.Text = attributes["kewarganegaraan"];
        }

        private void Algoritma(){

            Bitmap patternBMP = img;
            patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 1);

            string folderPath = "img";
            string[] bmpFiles = Directory.GetFiles(folderPath, "*.bmp", SearchOption.TopDirectoryOnly);

            double maxSimilarity = 0;
            var path = "";
            foreach (var bmpPath in bmpFiles)
            {

                if (algoritma == "KMP")
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

            // set path to temporary variable
            resultPath = path;
            // set similarity to temporary variable
            similariti = maxSimilarity;

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
                naem = value;
                if (dataMap.ContainsKey(name))
                {
                    // set the bio of the most similar fingerprint
                    attribute = dataMap[name];
                }
                else
                {
                    MessageBox.Show("Data tidak ditemukan");
                }
            }
            else
            {
                MessageBox.Show($"ID {namaFile} tidak ditemukan.");
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
                    img = new Bitmap(filename);
                    fingerImg.Visibility = Visibility.Collapsed;
                    txtFinger.Visibility = Visibility.Collapsed;

                    // Cek if the process has been done before or not
                    if (matchP.Visibility != Visibility.Visible)
                    {
                        foreach (var Pair in attribute)
                        {
                            attribute[Pair.Key] = "";
                        }

                        setBio(attribute);
                        nama.Text = "";

                        textMatch.Visibility = Visibility.Visible;
                        matchP.Visibility = Visibility.Visible;
                        IMGresult.Visibility = Visibility.Collapsed;

                        textMatch.Text = "Matching Print Will Be Here";
                        textMatch.Foreground = System.Windows.Media.Brushes.Black;
                        TimeTaken.Text = "-ms";
                        persen.Text = "-%";
                        resultPath = "";
                        similariti = 0;
                    }
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

        //private void deleteImg(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        imgUpld.Source = null;
        //        fingerImg.Visibility = Visibility.Visible;
        //        txtFinger.Visibility = Visibility.Visible;
        //        img = "";
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
    }
}