using System.Drawing;

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
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, cropWidth, cropHeight), new System.Drawing.Rectangle(startX, startY, cropWidth, cropHeight), GraphicsUnit.Pixel);
        }

        return croppedImage;
    }
}