using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using static AlayConverter;
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

class StringFormatter
{
   public string FormatString(string input)
   {
       // Mengganti angka dengan huruf yang sesuai menggunakan regex
       string replaced = Regex.Replace(input, @"\d+", match =>
       {
           switch (match.Value)
           {
               case "4":
                   return "a";
               case "1":
                   return "i";
               case "6":
                   return "g";
               case "9":
                   return "g";
               case "13":
                   return "b";
               default:
                   return "";
           }
       });

       // Membuat semua huruf menjadi kecil
       string lowerCase = replaced.ToLower();

       // Membuat huruf pertama dari setiap kata menjadi kapital
       TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
       string properCase = textInfo.ToTitleCase(lowerCase);

       return properCase;
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
//    static void Main()
//    {
//        // "../img/1.BMP"
//        Console.WriteLine("Selamat datang brooo");
//        Console.Write("Silahkan masukkan path gambar yang ingin disamakan: ");
//        string name = Console.ReadLine();
//        Console.WriteLine("Pilihan algoritma: ");
//        Console.WriteLine("1. KMP");
//        Console.WriteLine("2. BM");
//        Console.Write("Masukkan pilihan algoritma dengan angka: ");
//        string algo = Console.ReadLine();


//        Bitmap patternBMP = new Bitmap(name);
//        patternBMP = ImageProcessor.GetCenterCrop(patternBMP, patternBMP.Width, 1);

//        string folderPath = "../img";
//        string[] bmpFiles = Directory.GetFiles(folderPath, "*.bmp", SearchOption.TopDirectoryOnly);

//        double maxSimilarity = 0;
//        var path = "";
//        foreach (var bmpPath in bmpFiles)
//        {

//            if (algo == "1")
//            {
//                Bitmap bmp = new Bitmap(bmpPath);
//                string asciiArt = BitmapToBinaryAsciiConverter.ConvertToAscii(patternBMP);
//                string oir = BitmapToBinaryAsciiConverter.ConvertToAscii(bmp);
//                var result = KMPAlgorithm.KmpMatch(oir, asciiArt);
//                if (result.similarity == 1)
//                {
//                    maxSimilarity = result.similarity;
//                    path = bmpPath;
//                    break;
//                }
//                if (result.similarity > maxSimilarity)
//                {
//                    maxSimilarity = result.similarity;
//                    path = bmpPath;
//                }
//            }
//            else
//            {
//                Bitmap bmp = new Bitmap(bmpPath);
//                string asciiArt = BitmapToBinaryAsciiConverter.ConvertToAscii(patternBMP);
//                string oir = BitmapToBinaryAsciiConverter.ConvertToAscii(bmp);
//                double similarity = BoyerMoore.BmMatch(oir, asciiArt);
//                if (similarity == 1)
//                {
//                    maxSimilarity = similarity;
//                    path = bmpPath;
//                    break;
//                }
//                if (similarity > maxSimilarity)
//                {
//                    maxSimilarity = similarity;
//                    path = bmpPath;
//                }
//            }
//        }
//        Console.Write("path yang sama: ");
//        Console.WriteLine(path);
//        Console.Write("similarity: ");
//        Console.WriteLine(maxSimilarity);

//    }
    public static void Main(string[] args)
    {
        Console.Write("Masukkan nama lengkap: ");
        string fullName = Console.ReadLine();
        string randomAlayVersion = AlayConverter.ConvertAlay(fullName);
        Console.WriteLine("Nama alay: " + randomAlayVersion);
        Console.WriteLine("Nama Revert: " + AlayConverter.RevertAlay(randomAlayVersion));
    }

}



