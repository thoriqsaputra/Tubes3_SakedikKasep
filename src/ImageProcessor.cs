using System.Drawing;

public class ImageProcessor
{
    public static Bitmap GetCenterCrop(Bitmap originalImage, int cropWidth, int cropHeight)
    {
        int originalWidth = originalImage.Width;
        int originalHeight = originalImage.Height;

        int startX = (originalWidth - cropWidth) / 2;
        int startY = (originalHeight - cropHeight) / 2;

        startX = Math.Max(0, startX);
        startY = Math.Max(0, startY);

        cropWidth = Math.Min(cropWidth, originalWidth);
        cropHeight = Math.Min(cropHeight, originalHeight);

        Bitmap croppedImage = new Bitmap(cropWidth, cropHeight);

        using (Graphics g = Graphics.FromImage(croppedImage))
        {
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, cropWidth, cropHeight), new System.Drawing.Rectangle(startX, startY, cropWidth, cropHeight), GraphicsUnit.Pixel);
        }

        return croppedImage;
    }
}