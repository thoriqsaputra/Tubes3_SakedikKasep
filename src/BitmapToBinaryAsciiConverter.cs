using System.Drawing;
using System.Text;

public class BitmapToBinaryAsciiConverter
{

    public static String ConvertToAscii(Bitmap original, int threshold = 128)
    {
        Bitmap binaryImage = new Bitmap(original.Width, original.Height);

        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                System.Drawing.Color originalColor = original.GetPixel(x, y);
                int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                System.Drawing.Color binaryColor = grayScale < threshold ? System.Drawing.Color.Black : System.Drawing.Color.White;
                binaryImage.SetPixel(x, y, binaryColor);
            }
        }

        StringBuilder binaryStringBuilder = new StringBuilder();
        StringBuilder asciiArt = new StringBuilder();

        for (int y = 0; y < binaryImage.Height; y++)
        {
            for (int x = 0; x < binaryImage.Width; x++)
            {
                System.Drawing.Color pixelColor = binaryImage.GetPixel(x, y);
                binaryStringBuilder.Append(pixelColor.R == 0 ? '0' : '1');
            }
        }

        string binaryString = binaryStringBuilder.ToString();

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