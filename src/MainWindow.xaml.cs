﻿using System.Media;
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

    public partial class MainWindow : Window
    {
        // user inputs
        public string algoritma;
        public Bitmap img;

        // database
        private Dictionary<string, string> sidikJariMap;
        private Dictionary<string, Dictionary<string, string>> dataMap;

        // temporary variables for results
        private Dictionary<string, string> attribute;
        private double similariti;
        private string naem;
        private String resultPath = "";
        private Boolean found;

        public MainWindow()
        {
            InitializeComponent();

            string connectionString = "Data Source=biodata_encrypted.db;Version=3;";
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
                                { "NIK", DecryptCaesarCipher(reader["NIK"].ToString(), 4) },
                                { "tempat_lahir", DecryptCaesarCipher(reader["tempat_lahir"].ToString(), 4) },
                                { "tanggal_lahir", reader["tanggal_lahir"].ToString() },
                                { "jenis_kelamin", DecryptCaesarCipher(reader["jenis_kelamin"].ToString(), 4) },
                                { "golongan_darah", DecryptCaesarCipher(reader["golongan_darah"].ToString(), 4) },
                                { "alamat", DecryptCaesarCipher(reader["alamat"].ToString(), 4) },
                                { "agama", DecryptCaesarCipher(reader["agama"].ToString(), 4) },
                                { "status_perkawinan", DecryptCaesarCipher(reader["status_perkawinan"].ToString(), 4) },
                                { "pekerjaan", DecryptCaesarCipher(reader["pekerjaan"].ToString(), 4) },
                                { "kewarganegaraan", DecryptCaesarCipher(reader["kewarganegaraan"].ToString(), 4) }
                            };
                                // Tambahkan ke dalam Dictionary dataMap
                                dataMap.Add(nama, attributes);
                            }
                        }
                    }
                }
            }

        }

        private string DecryptCaesarCipher(string text, int shift)
        {
            return CaesarCipher(text, -shift);
        }

        private string CaesarCipher(string text, int shift)
        {
            string result = "";
            shift = (shift % 26 + 26) % 26;  // Ensure shift is within 0-25

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char d = char.IsUpper(c) ? 'A' : 'a';
                    result += (char)((c - d + shift) % 26 + d);
                }
                else if (char.IsDigit(c))
                {
                    result += (char)((c - '0' + shift) % 10 + '0');
                }
                else
                {
                    result += c;
                }
            }
            return result;
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

            // set the found to false
            found = false;

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
            if (!found) // REMBER TO ADD SOME BOOLEAN EXPRESSION WETHER THE IMAGE IS FOUND OR NOT
            { 
                textMatch.Text = "Not Match";
                textMatch.Foreground = System.Windows.Media.Brushes.Red;
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

            // show the text
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
            patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 5);

            string folderPath = "../test/img";
            string[] bmpFiles = Directory.GetFiles(folderPath, "*.bmp", SearchOption.TopDirectoryOnly);

            double maxSimilarity = 0;
            var path = "";
            bool exactMatchFound = false;

            foreach (var bmpPath in bmpFiles)
            {
                Bitmap bmp = new Bitmap("../test/" + bmpPath);
                string asciiArt = BitmapToBinaryAsciiConverter.ConvertToAscii(patternBMP);
                string oir = BitmapToBinaryAsciiConverter.ConvertToAscii(bmp);
                if (algoritma == "KMP")
                {
                    var result = KMPAlgorithm.KmpMatch(oir, asciiArt);
                    if (result.similarity == 1)
                    {
                        exactMatchFound = true;
                        maxSimilarity = result.similarity; // Exact match found
                        path = bmpPath;
                        break; // No need to check further if an exact match is found
                    }

                    if (result.similarity > maxSimilarity)
                    {
                        maxSimilarity = result.similarity;
                        path = bmpPath;
                    }
                }
                else
                {
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
                        // empty the bio
                        foreach (var Pair in attribute)
                        {
                            attribute[Pair.Key] = "";
                        }

                        if (attribute[attribute.Keys.First()] == "")
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