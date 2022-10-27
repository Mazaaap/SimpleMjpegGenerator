using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;


namespace SimpleMjpegGenerator.Controllers;


[ApiController]
public sealed class MjpegController : ControllerBase
{
    private readonly IJpegGeneratorService _jpegGeneratorService;

    public MjpegController(IJpegGeneratorService jpegGeneratorService)
    {
        _jpegGeneratorService = jpegGeneratorService;
    }


    [HttpGet("/")]
    public async Task GenerateMJpegStream(int? fps)
    {
        //context.Features.Get<IHttpResponseBodyFeature>().DisableBuffering();  // no effect?
        HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
        HttpContext.Response.Headers.Add("Pragma", "no-cache");
        HttpContext.Response.ContentType = "multipart/x-mixed-replace;boundary=" + _jpegGeneratorService.GetBoundary();
        HttpContext.Response.StatusCode = 200;
        var cancellationToken = HttpContext.RequestAborted;

        await using var bodyStream = HttpContext.Response.BodyWriter.AsStream();
        await HttpContext.Response.StartAsync();

        var delayMs = 1;
        if (fps != null && fps.Value > 1 && fps.Value <= 60)
        {
            delayMs = 1000 / fps.Value;
        }

        while (true)
        {
            var timeStamp = _jpegGeneratorService.GetTimeStamp().ToString("o");
            var image = _jpegGeneratorService.GetImage();
            Console.WriteLine("Length: " + image.Length +"  TimeStamp: " + timeStamp);

            if (image.Length > 0)
            {
                try
                {
                    await bodyStream.WriteAsync(image, cancellationToken);
                    await bodyStream.FlushAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("HTTP write error:\n" + e);
                    break;
                }
            }
            await Task.Delay(delayMs);

        }
        Console.WriteLine("MJPEG request end!");
    }
}
