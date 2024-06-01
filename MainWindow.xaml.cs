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
using System.Security.Cryptography;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;

namespace Tubes3_SakedikKasep
{
    public partial class MainWindow : Window
    {
        // user inputs
        public string? algoritma;
        public Bitmap? img;

        // database
        private Dictionary<string, string> sidikJariMap;
        private Dictionary<string, Dictionary<string, string>> dataMap;

        // temporary variables for results
        private Dictionary<string, string>? attribute;
        private double similariti;
        private string? naem;
        private string resultPath = "";
        private bool found;

        // RSA Keys
        private RSA privateKey;
        private RSA publicKey;

        public MainWindow()
        {
            InitializeComponent();

            string connectionString = "Data Source=biodata1.db; Version=3;";
            sidikJariMap = new Dictionary<string, string>();
            dataMap = new Dictionary<string, Dictionary<string, string>>();

            // Load RSA keys
            privateKey = RSA.Create();
            publicKey = RSA.Create();

            privateKey.ImportFromPem(File.ReadAllText("Key/private_key.pem"));
            publicKey.ImportFromPem(File.ReadAllText("Key/public_key.pem"));

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM sidik_jari";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader.GetString(0);
                            string name = Decrypt(reader.GetString(1) ?? string.Empty);

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
                            string nama = Decrypt(reader["nama"]?.ToString() ?? string.Empty);

                            if (!dataMap.ContainsKey(nama))
                            {
                                Dictionary<string, string> attributes = new Dictionary<string, string>
                                {
                                    { "NIK", reader["NIK"]?.ToString() ?? string.Empty },
                                    { "tempat_lahir", Decrypt(reader["tempat_lahir"]?.ToString() ?? string.Empty) },
                                    { "tanggal_lahir", Decrypt(reader["tanggal_lahir"]?.ToString() ?? string.Empty) },
                                    { "jenis_kelamin", Decrypt(reader["jenis_kelamin"]?.ToString() ?? string.Empty) },
                                    { "golongan_darah", Decrypt(reader["golongan_darah"]?.ToString() ?? string.Empty) },
                                    { "alamat", Decrypt(reader["alamat"]?.ToString() ?? string.Empty) },
                                    { "agama", Decrypt(reader["agama"]?.ToString() ?? string.Empty) },
                                    { "status_perkawinan", Decrypt(reader["status_perkawinan"]?.ToString() ?? string.Empty) },
                                    { "pekerjaan", Decrypt(reader["pekerjaan"]?.ToString() ?? string.Empty) },
                                    { "kewarganegaraan", Decrypt(reader["kewarganegaraan"]?.ToString() ?? string.Empty) }
                                };
                                dataMap.Add(nama, attributes);
                            }
                        }
                    }
                }
            }
        }

        private string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private async void runAlgoritma(Object sender, RoutedEventArgs eventArgs)
        {
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

            found = false;
            loading.Visibility = Visibility.Visible;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.Run(() => Algoritma());

            stopwatch.Stop();
            long elapsed_time = stopwatch.ElapsedMilliseconds;

            loading.Visibility = Visibility.Collapsed;
            textMatch.Visibility = Visibility.Collapsed;
            matchP.Visibility = Visibility.Collapsed;

            if (!found)
            {
                textMatch.Text = "Not Match";
                textMatch.Foreground = System.Windows.Media.Brushes.Red;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("Image/sad.gif", UriKind.Relative);
                bitmap.EndInit();
                IMGresult.Source = bitmap;
                IMGresult.Visibility = Visibility.Visible;
            }
            else
            {
                setBio(attribute ?? new Dictionary<string, string>());
                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string absolutePath = System.IO.Path.Combine(projectDirectory, resultPath);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(absolutePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                IMGresult.Source = bitmap;
                IMGresult.Visibility = Visibility.Visible;
                textMatch.Text = "Match";
                textMatch.Foreground = System.Windows.Media.Brushes.Green;
            }

            textMatch.Visibility = Visibility.Visible;
            TimeTaken.Text = elapsed_time < 1000 ? $"{elapsed_time} ms" : elapsed_time < 60000 ? $"{elapsed_time / 1000} s" : $"{elapsed_time / 60000} m";
            persen.Text = $"{similariti * 100}%";
        }

        public void setBio(Dictionary<string, string> attributes)
        {
            NIK.Text = attributes["NIK"];
            nama.Text = naem ?? string.Empty;
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

        private void Algoritma()
        {
            Bitmap? patternBMP = img;
            if (patternBMP == null)
            {
                return;
            }

            patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 1);

            string folderPath = "img";
            string[] bmpFiles = Directory.GetFiles(folderPath, "*.bmp", SearchOption.TopDirectoryOnly);

            double maxSimilarity = 0;
            string path = "";
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

            resultPath = path;
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
                    attribute = dataMap[name];
                    found = true;
                }
                else
                {
                    MessageBox.Show(name);
                    MessageBox.Show("Data tidak ditemukan");
                    found = false;
                }
            }
            else
            {
                MessageBox.Show($"ID {namaFile} tidak ditemukan.");
                found = false;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void uploadImage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".jpg";
                dlg.Filter = "Image Files |*.bmp;*.jpg;*.png";
                bool? result = dlg.ShowDialog();
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

                    if (matchP.Visibility != Visibility.Visible)
                    {
                        attribute = new Dictionary<string, string>();

                        if (attribute.Count > 0 && attribute[attribute.Keys.First()] != "")
                        {
                            setBio(attribute);
                            nama.Text = "";
                        }

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
    }
}
