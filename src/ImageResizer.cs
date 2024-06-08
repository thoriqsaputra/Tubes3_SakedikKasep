using System.Drawing;

public class ImageResizer
{
    public static Bitmap ResizeImage(string sourcePath, int width, int height)
    {
        using (Bitmap originalImage = new Bitmap(sourcePath))
        {
            Bitmap resizedImage = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, width, height);
            }

            return resizedImage;
        }
    }
}