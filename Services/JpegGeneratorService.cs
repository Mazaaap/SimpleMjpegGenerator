using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Hosting;
using System;


public interface IJpegGeneratorService
{
    string GetBoundary();
    byte[] GetImage();
    DateTime GetTimeStamp();
}
public class JpegGeneratorService : BackgroundService, IJpegGeneratorService
{
    private const string _boundary = "myBoundary";
    private byte[] _generatedImage = new byte[0];
    private DateTime _timeStamp = DateTime.MinValue;
    private readonly Random _rand = new Random();

    private readonly TimeSpan _periodTimeSpan = TimeSpan.FromMilliseconds(20);


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_periodTimeSpan);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            GenerateImage();
        }
    }


    public string GetBoundary()
    {
        return _boundary;
    }

    public byte[] GetImage()
    {
        return _generatedImage;
    }

    public DateTime GetTimeStamp()
    {
        return _timeStamp;
    }

    private void GenerateImage()
	{
        const int width = 1280;
        const int height = 720;

        Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        Graphics g = Graphics.FromImage(bmp);

        const int size = 10;
        for (int y = 0; y < height; y += size)
        {
            for (int x = 0; x < width; x += size)
            {
                int red = _rand.Next(128, 240);
                int green = _rand.Next(128, 240);
                int blue = _rand.Next(128, 240);

                g.FillRectangle(
                    new SolidBrush(Color.FromArgb(255, red, green, blue)),
                    x, y, size, size
                );
            }
        }
        g.DrawString(
            "JPEG " + DateTime.UtcNow.ToString("o"),
            new Font("Arial", 48, FontStyle.Bold),
            SystemBrushes.WindowText,
            new PointF(60, 60)
        );

        var bitmapAsByteArray = ToByteArray(bmp, ImageFormat.Jpeg);
        g.Dispose();
        bmp.Dispose();



        string header = "--" + _boundary + "\r\nContent-Type:image/jpeg\r\nContent-Length:" + bitmapAsByteArray.Length.ToString() + "\r\n\r\n";

        _timeStamp = DateTime.UtcNow;
        Array.Resize<byte>(ref _generatedImage, (header.Length+bitmapAsByteArray.Length));
        _generatedImage = CombineByteArrays(Encoding.ASCII.GetBytes(header), bitmapAsByteArray);
    }

    public static byte[] ToByteArray(Image image, ImageFormat format)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, format);
            return ms.ToArray();
        }
    }
    public static byte[] CombineByteArrays(byte[] first, byte[] second)
    {
        byte[] ret = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        return ret;
    }
}

