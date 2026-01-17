using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Storage;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace Northwind.AzureFunctions.Service;

public class CheckGeneratorFunction
{
    private readonly ILogger<CheckGeneratorFunction> _logger;

    public CheckGeneratorFunction(ILogger<CheckGeneratorFunction> logger)
    {
        _logger = logger;
    }


    [Function(nameof(CheckGeneratorFunction))]
    [BlobOutput("checks-blob-container/check.png")] // ${DateTime.Now.Ticks.ToString()}-
    public byte[] Run([QueueTrigger("checks-queue", Connection = "")] QueueMessage message)
        {
        _logger.LogInformation("C# Queue trigger function executed.");
        _logger.LogInformation($"Body: {message}.");
        // Create a new blank image with a white background.
        using (Image<Rgba32> image = new(width: 1200, height: 600,
              backgroundColor: new Rgba32(r: 255, g: 255, b: 255, a: 100)))
        {
            // Load the font file and create a large font.
            FontCollection collection = new();
            FontFamily family = collection.Add(
              @"fonts\Caveat\static\Caveat-Regular.ttf");
            Font font = family.CreateFont(72);
            string amount = message.Body.ToString();
            DrawingOptions options = new()
            {
                GraphicsOptions = new()
                {
                    ColorBlendingMode = PixelColorBlendingMode.Multiply
                }
            };
            // Define some pens and brushes.
            Pen blackPen = Pens.Solid(Color.Black, 2);
            Pen blackThickPen = Pens.Solid(Color.Black, 8);
            Pen greenPen = Pens.Solid(Color.Green, 3);
            Brush redBrush = Brushes.Solid(Color.Red);
            Brush blueBrush = Brushes.Solid(Color.Blue);
            // Define some paths and draw them.
            IPath border = new RectangularPolygon(
              x: 50, y: 50, width: 1100, height: 500);
            image.Mutate(x => x.Draw(options, blackPen, border));
            IPath star = new Star(x: 150.0f, y: 150.0f,
              prongs: 5, innerRadii: 20.0f, outerRadii: 30.0f);
            image.Mutate(x => x.Fill(options, redBrush, star)
                               .Draw(options, greenPen, star));
            IPath line1 = new Polygon(new LinearLineSegment(
              new PointF(x: 100, y: 275), new PointF(x: 1050, y: 275)));

            image.Mutate(x => x.Draw(options, blackPen, line1));
            IPath line2 = new Polygon(new LinearLineSegment(
              new PointF(x: 100, y: 365), new PointF(x: 1050, y: 365)));
            image.Mutate(x => x.Draw(options, blackPen, line2));
            RichTextOptions textOptions = new(font)
            {
                Origin = new PointF(100, 200),
                WrappingLength = 1000,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            image.Mutate(x => x.DrawText(
              textOptions, amount, blueBrush, blackPen));
            string blobName = $"{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}.png";
            _logger.LogInformation($"Blob filename: {blobName}.");
            try
            {
                if (Environment.GetEnvironmentVariable("IS_LOCAL") == "true")
                {
                    // Create blob in the local filesystem.
                    string folder = $@"{System.Environment.CurrentDirectory}\blobs";
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    _logger.LogInformation($"Blobs folder: {folder}");
                    string blobPath = $@"{folder}\{blobName}";
                    image.SaveAsPng(blobPath);
                }
                // Create BLOB in Blob Storage via a memory stream.
                MemoryStream stream = new();
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Array.Empty<byte>();
        }
    }
}
