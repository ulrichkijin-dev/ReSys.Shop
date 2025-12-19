using ReSys.Shop.Core.Common.Services.Storage.Models;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ReSys.Shop.Infrastructure.Storages.Helpers;

internal static class ImageProcessingHelper
{
    public static string BuildPath(this UploadOptions options, string originalName, string ext)
    {
        string fileNameWithoutExt;

        if (options.FileName != null)
        {
            fileNameWithoutExt = options.FileName;
        }
        else if (options.PreserveOriginalFileName)
        {
            fileNameWithoutExt = Path.GetFileNameWithoutExtension(path: originalName);
        }
        else
        {
            fileNameWithoutExt = $"{DateTimeOffset.UtcNow.Ticks}_{Guid.NewGuid():N}";
        }

        var folder = options.Folder?.TrimEnd(trimChars: ['/', '\\']) ?? string.Empty;
        var fullPath = Path.Combine(path1: folder, path2: $"{fileNameWithoutExt}{ext}");

        return fullPath.Replace(oldChar: '\\', newChar: '/');
    }
    public static async Task<(Stream ProcessedStream, int Width, int Height, string ContentType)> ProcessImageAsync(
        Stream originalStream,
        UploadOptions options,
        CancellationToken ct)
    {
        originalStream.Position = 0;
        using var image = await Image.LoadAsync(stream: originalStream, cancellationToken: ct);

        int width = image.Width;
        int height = image.Height;

        if (options.MaxDimensions.HasValue)
        {
            var (targetW, targetH) = options.MaxDimensions.Value;

            bool needsResize = width > targetW || height > targetH;
            bool shouldUpscale = options.AllowUpscale && (width < targetW || height < targetH);

            if (needsResize || shouldUpscale)
            {
                ResizeMode mode = options.CropToFit ? ResizeMode.Crop : ResizeMode.Max;

                image.Mutate(operation: x => x.Resize(options: new ResizeOptions
                {
                    Size = new Size(width: targetW, height: targetH),
                    Mode = mode,
                    Position = AnchorPositionMode.Center
                }));

                width = image.Width;
                height = image.Height;
            }
        }

        if (options.OptimizeImage)
        {
            image.Mutate(operation: x => x.AutoOrient());
        }

        var encoder = options.ConvertToWebP
            ? (IImageEncoder)new WebpEncoder { Quality = options.Quality }
            : new JpegEncoder { Quality = options.Quality };

        var memoryStream = new MemoryStream();
        await image.SaveAsync(stream: memoryStream, encoder: encoder, cancellationToken: ct);
        memoryStream.Position = 0;

        string contentType = options.ConvertToWebP ? "image/webp" : "image/jpeg";

        return (memoryStream, width, height, contentType);
    }

    public static async Task<MemoryStream> GenerateThumbnailAsync(
        Stream originalStream,
        int targetWidth,
        int quality,
        CancellationToken ct)
    {
        originalStream.Position = 0;
        using var image = await Image.LoadAsync(stream: originalStream, cancellationToken: ct);

        image.Mutate(operation: ctx => ctx.Resize(options: new ResizeOptions
        {
            Size = new Size(width: targetWidth, height: targetWidth),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        var stream = new MemoryStream();
        await image.SaveAsWebpAsync(stream: stream, encoder: new WebpEncoder { Quality = quality }, cancellationToken: ct);
        stream.Position = 0;
        return stream;
    }
}