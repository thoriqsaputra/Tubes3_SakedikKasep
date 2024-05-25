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
public class ImageResizer
{
    public static Bitmap ResizeImage(string sourcePath, int width, int height)
    {
        // Memuat gambar asli
        using (Bitmap originalImage = new Bitmap(sourcePath))
        {
            // Membuat bitmap baru dengan ukuran yang ditentukan
            Bitmap resizedImage = new Bitmap(width, height);

            // Menggunakan Graphics untuk meresize gambar
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                // Menetapkan kualitas gambar
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // Menggambar gambar yang telah diresize
                graphics.DrawImage(originalImage, 0, 0, width, height);
            }

            // Mengembalikan gambar yang telah diresize
            return resizedImage;
        }
    }
}

public class BitmapToBinaryAsciiConverter
{

    public static String ConvertToAscii(Bitmap original, int threshold = 128)
    {
        Bitmap binaryImage = new Bitmap(original.Width, original.Height);

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                // Mendapatkan warna piksel
                Color originalColor = original.GetPixel(x, y);
                // Mengubah warna piksel menjadi grayscale
                int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                // Mengubah piksel menjadi hitam atau putih berdasarkan threshold
                Color binaryColor = grayScale < threshold ? Color.Black : Color.White;
                binaryImage.SetPixel(x, y, binaryColor);
            }
        }

        StringBuilder binaryStringBuilder = new StringBuilder();
        StringBuilder asciiArt = new StringBuilder();

        // Mengonversi gambar biner menjadi string biner
        for (int y = 0; y < binaryImage.Height; y++)
        {
            for (int x = 0; x < binaryImage.Width; x++)
            {
                Color pixelColor = binaryImage.GetPixel(x, y);
                binaryStringBuilder.Append(pixelColor.R == 0 ? '0' : '1');
            }
        }

        string binaryString = binaryStringBuilder.ToString();

        // Mengonversi setiap 8 bit biner menjadi karakter ASCII
        for (int i = 0; i < binaryString.Length; i += 8)
        {
            if (i + 8 <= binaryString.Length)
            {
                string byteString = binaryString.Substring(i, 8);
                byte asciiByte = Convert.ToByte(byteString, 2);
                asciiArt.Append((char)asciiByte);
            }
        }

        return asciiArt.ToString();
    }
}

public class BoyerMoore
{
    public static double BmMatch(string text, string pattern)
    {
        Dictionary<int, int> last = BuildLast(pattern);
        int n = text.Length;
        int m = pattern.Length;
        int i = m - 1;

        if (i > n - 1)
            return -1;

        int j = m - 1;

        do
        {
            if (pattern[j] == text[i])
            {
                if (j == 0)
                {
                    // Console.WriteLine(CalculateLevenshteinSimilarity(text,pattern));
                    return 1;
                }
                else
                {
                    i--;
                    j--;
                }
            }
            else
            {
                int lo = last.ContainsKey(text[i]) ? last[text[i]] : -1;
                i = i + m - Math.Min(j, 1 + lo);
                j = m - 1;
            }
        } while (i <= n - 1);

        return CalculateLevenshteinSimilarity(text, pattern);
    }

    // Metode untuk membangun tabel 'last' untuk pola yang diberikan
    public static Dictionary<int, int> BuildLast(string pattern)
    {
        Dictionary<int, int> last = new Dictionary<int, int>();

        // Mengisi tabel dengan posisi terakhir dari setiap karakter dalam pola
        for (int i = 0; i < pattern.Length; i++)
            last[pattern[i]] = i;

        return last;
    }

    public static double CalculateLevenshteinSimilarity(string str1, string str2)
    {
        int len1 = str1.Length;
        int len2 = str2.Length;

        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= len2; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        int maxLen = Math.Max(len1, len2);
        double similarity = 1.0 - (double)dp[len1, len2] / maxLen;

        return similarity * 100.0; // Dalam bentuk persentase
    }
}

public static class KMPAlgorithm
{
    public static (double similarity, int position) KmpMatch(string text, string pattern)
    {
        int n = text.Length;
        int m = pattern.Length;
        int[] b = ComputeBorder(pattern);
        int i = 0;
        int j = 0;

        double maxSimilarity = 0;
        int bestPosition = -1;

        while (i < n)
        {
            if (pattern[j] == text[i])
            {
                if (j == m - 1)
                {
                    double similarity = CalculateLevenshteinSimilarity(text.Substring(i - m + 1, m), pattern);
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestPosition = i - m + 1;
                    }
                    j = b[j];
                }
                else
                {
                    i++;
                    j++;
                }
            }
            else if (j > 0)
            {
                j = b[j - 1];
            }
            else
            {
                i++;
            }
        }

        return (maxSimilarity, bestPosition);
    }

    public static int[] ComputeBorder(string pattern)
    {
        int[] b = new int[pattern.Length];
        b[0] = 0;

        int m = pattern.Length;
        int j = 0;
        int i = 1;

        while (i < m)
        {
            if (pattern[i] == pattern[j])
            {
                j++;
                b[i] = j;
                i++;
            }
            else
            {
                if (j != 0)
                {
                    j = b[j - 1];
                }
                else
                {
                    b[i] = 0;
                    i++;
                }
            }
        }
        return b;
    }

    public static double CalculateLevenshteinSimilarity(string str1, string str2)
    {
        int len1 = str1.Length;
        int len2 = str2.Length;
        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= len2; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        int maxLen = Math.Max(len1, len2);
        double similarity = 1.0 - (double)dp[len1, len2] / maxLen;
        return similarity;
    }
}
public class ImageProcessor
{
    public static Bitmap GetCenterCrop(Bitmap originalImage, int cropWidth, int cropHeight)
    {
        int originalWidth = originalImage.Width;
        int originalHeight = originalImage.Height;

        // Menentukan posisi awal untuk crop di tengah-tengah gambar
        int startX = (originalWidth - cropWidth) / 2;
        int startY = (originalHeight - cropHeight) / 2;

        // Memastikan startX dan startY tidak negatif
        startX = Math.Max(0, startX);
        startY = Math.Max(0, startY);

        // Memastikan cropWidth dan cropHeight tidak lebih besar dari ukuran gambar asli
        cropWidth = Math.Min(cropWidth, originalWidth);
        cropHeight = Math.Min(cropHeight, originalHeight);

        // Membuat Bitmap baru untuk gambar hasil crop
        Bitmap croppedImage = new Bitmap(cropWidth, cropHeight);

        using (Graphics g = Graphics.FromImage(croppedImage))
        {
            g.DrawImage(originalImage, new Rectangle(0, 0, cropWidth, cropHeight), new Rectangle(startX, startY, cropWidth, cropHeight), GraphicsUnit.Pixel);
        }

        return croppedImage;
    }
}

class Program
{
    public static void Main(string[] args)
    {
        // Connection string ke database SQLite
        string connectionString = "Data Source=biodata.db;Version=3;";
        Dictionary<string, string> sidikJariMap = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> dataMap = new Dictionary<string, Dictionary<string, string>>();

        // Membuat koneksi ke database
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
            string pathSearch = Console.ReadLine();
            Console.WriteLine("Pilihan algoritma: ");
            Console.WriteLine("1. KMP");
            Console.WriteLine("2. BM");
            Console.Write("Masukkan pilihan algoritma dengan angka: ");
            string algo = Console.ReadLine();


            Bitmap patternBMP = new Bitmap(pathSearch);
            patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 1);

            string folderPath = "../img";
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
            string namaFile = Path.GetFileNameWithoutExtension(path);

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

}



