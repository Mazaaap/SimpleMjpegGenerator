using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

/*
builder.WebHost.UseKestrel();
builder.WebHost.UseSockets(socketTransportOptions =>
{
    socketTransportOptions.MaxWriteBufferSize = ???;
    socketTransportOptions.WaitForDataBeforeAllocatingBuffer = ???;
});
*/

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

var jpegGeneratorService = new JpegGeneratorService();

builder.Services.AddSingleton<IJpegGeneratorService>(jpegGeneratorService);
builder.Services.AddHostedService(_ => jpegGeneratorService);
builder.Services.AddControllers();


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
    serverOptions.Limits.MaxConcurrentConnections = 100;
    /*
    serverOptions.Limits.MinRequestBodyDataRate = new MinDataRate(
        bytesPerSecond: 10000, gracePeriod: TimeSpan.FromSeconds(10));
    serverOptions.Limits.MinResponseDataRate = new MinDataRate(
        bytesPerSecond: 10000, gracePeriod: TimeSpan.FromSeconds(10));
*/
    //serverOptions.Limits.MaxResponseBufferSize = 0;  //deprecated??
    // https://github.com/dotnet/aspnetcore/issues/26565
    // https://github.com/dotnet/aspnetcore/issues/12222
});

var app = builder.Build();
app.MapControllers();
app.UseHttpLogging();
app.Run();

