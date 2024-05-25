using System.Drawing;

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