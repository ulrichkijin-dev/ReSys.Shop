using Google.Cloud.Storage.V1;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using ReSys.Shop.Core.Common.Services.Storage.Models;
using ReSys.Shop.Core.Common.Services.Storage.Services;
using ReSys.Shop.Infrastructure.Storages.Helpers;
using ReSys.Shop.Infrastructure.Storages.Options;

using Serilog;

namespace ReSys.Shop.Infrastructure.Storages.Providers;

public sealed class GoogleCloudStorageService : IStorageService
{
    private readonly StorageClient _client;
    private readonly StorageOptions _options;

    public GoogleCloudStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;

        if (!string.IsNullOrEmpty(value: _options.GoogleCredentialsPath))
        {
            Environment.SetEnvironmentVariable(
                variable: "GOOGLE_APPLICATION_CREDENTIALS",
                value: _options.GoogleCredentialsPath);
        }

        _client = StorageClient.Create();
    }

    public async Task<ErrorOr<StorageFileInfo>> UploadFileAsync(
        IFormFile? file,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= UploadOptions.Default;

        if (file is null)
            return StorageErrors.FileEmpty;

        if (file.Length == 0)
            return StorageErrors.FileEmptyContent;

        if (file.Length > _options.MaxFileSizeBytes)
            return StorageErrors.FileTooLarge(maxSizeBytes: _options.MaxFileSizeBytes);

        var ext = Path.GetExtension(path: file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(value: ext))
            return StorageErrors.FileInvalidType(extension: ext, allowed: _options.AllowedExtensions);

        try
        {
            var finalExt = options.ConvertToWebP ? ".webp" : ext;
            var path = options.BuildPath(originalName: file.FileName, ext: finalExt);

            await using var input = file.OpenReadStream();

            Stream uploadStream = input;
            int? width = null;
            int? height = null;
            string contentType = file.ContentType;

            if (file.ContentType.StartsWith(value: "image/", comparisonType: StringComparison.OrdinalIgnoreCase)
                && options.OptimizeImage)
            {
                var processed =
                    await ImageProcessingHelper.ProcessImageAsync(originalStream: input, options: options, ct: cancellationToken);

                uploadStream = processed.ProcessedStream;
                width = processed.Width;
                height = processed.Height;
                contentType = processed.ContentType;
            }

            await _client.UploadObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: path,
                contentType: contentType,
                source: uploadStream,
                cancellationToken: cancellationToken);

            var thumbnails = await UploadThumbnailsAsync(
                original: input,
                options: options,
                basePath: path,
                ct: cancellationToken);

            return new StorageFileInfo
            {
                Path = path,
                Url = GetFileUrl(path: path),
                ContentType = contentType,
                Length = uploadStream.Length,
                Width = width,
                Height = height,
                Thumbnails = thumbnails,
                Metadata = options.Metadata,
                LastModified = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            Log.Error(exception: ex, messageTemplate: "Google Cloud upload failed");
            return StorageErrors.UploadFailed(reason: ex.Message);
        }
    }

    public async Task<ErrorOr<IReadOnlyList<StorageFileInfo>>> UploadBatchAsync(
        IEnumerable<IFormFile> files,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<StorageFileInfo>();

        foreach (var file in files)
        {
            var r = await UploadFileAsync(file: file, options: options, cancellationToken: cancellationToken);
            if (r.IsError)
                return r.Errors;

            results.Add(item: r.Value);
        }

        return results.AsReadOnly();
    }

    public async Task<ErrorOr<Success>> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = ExtractPath(fileUrl: fileUrl);

        try
        {
            await _client.DeleteObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: path,
                cancellationToken: cancellationToken);

            return Result.Success;
        }
        catch (Google.GoogleApiException ex) when (ex.Error?.Code == 404)
        {
            return StorageErrors.FileNotFound(path: path);
        }
    }

    public async Task<ErrorOr<Success>> DeleteBatchAsync(
        IEnumerable<string> fileUrls,
        CancellationToken cancellationToken = default)
    {
        foreach (var url in fileUrls)
        {
            var r = await DeleteFileAsync(fileUrl: url, cancellationToken: cancellationToken);
            if (r.IsError)
                return r.Errors;
        }

        return Result.Success;
    }

    public async Task<ErrorOr<Stream>> GetFileStreamAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = ExtractPath(fileUrl: fileUrl);
        var ms = new MemoryStream();

        try
        {
            await _client.DownloadObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: path,
                destination: ms,
                cancellationToken: cancellationToken);

            ms.Position = 0;
            return ms;
        }
        catch (Google.GoogleApiException ex) when (ex.Error?.Code == 404)
        {
            return StorageErrors.FileNotFound(path: path);
        }
    }

    public async Task<ErrorOr<bool>> ExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = ExtractPath(fileUrl: fileUrl);

        try
        {
            await _client.GetObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: path,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.Error?.Code == 404)
        {
            return false;
        }
    }

    public Task<ErrorOr<IReadOnlyList<StorageFileMetadata>>> ListFilesAsync(
        string? prefix = null,
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        var results = new List<StorageFileMetadata>();

        var objects = _client.ListObjects(
            bucket: _options.GoogleBucketName!,
            prefix: prefix);

        foreach (var obj in objects)
        {
            if (!recursive && obj.Name.Contains(value: '/'))
                continue;

            results.Add(item: new StorageFileMetadata
            {
                Path = obj.Name,
                Url = GetFileUrl(path: obj.Name),
                Length = (long)(obj.Size ?? 0UL),
                LastModified = obj.UpdatedDateTimeOffset ?? DateTimeOffset.UtcNow,
                ContentType = obj.ContentType
            });
        }

        return Task.FromResult<ErrorOr<IReadOnlyList<StorageFileMetadata>>>(
            result: results.AsReadOnly());
    }


    public async Task<ErrorOr<StorageFileInfo>> CopyFileAsync(
        string sourceUrl,
        string destinationPath,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        var sourcePath = ExtractPath(fileUrl: sourceUrl);

        try
        {
            var obj = await _client.CopyObjectAsync(
                sourceBucket: _options.GoogleBucketName!,
                sourceObjectName: sourcePath,
                destinationBucket: _options.GoogleBucketName!,
                destinationObjectName: destinationPath,
                cancellationToken: cancellationToken);

            return new StorageFileInfo
            {
                Path = destinationPath,
                Url = GetFileUrl(path: destinationPath),
                ContentType = obj.ContentType ?? "application/octet-stream",
                Length = (long)(obj.Size ?? 0UL),
                LastModified = obj.UpdatedDateTimeOffset ?? DateTimeOffset.UtcNow
            };
        }
        catch (Google.GoogleApiException ex) when (ex.Error?.Code == 404)
        {
            return StorageErrors.FileNotFound(path: sourcePath);
        }
    }

    public async Task<ErrorOr<StorageFileInfo>> MoveFileAsync(
        string sourceUrl,
        string destinationPath,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        var copy = await CopyFileAsync(
            sourceUrl: sourceUrl,
            destinationPath: destinationPath,
            overwrite: overwrite,
            cancellationToken: cancellationToken);

        if (copy.IsError)
            return copy.Errors;

        await DeleteFileAsync(fileUrl: sourceUrl, cancellationToken: cancellationToken);
        return copy.Value;
    }

    public async Task<ErrorOr<StorageFileInfo>> GetMetadataAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = ExtractPath(fileUrl: fileUrl);

        try
        {
            var obj = await _client.GetObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: path,
                cancellationToken: cancellationToken);

            return new StorageFileInfo
            {
                Path = obj.Name,
                Url = GetFileUrl(path: obj.Name),
                ContentType = obj.ContentType ?? "application/octet-stream",
                Length = (long)(obj.Size ?? 0UL),
                LastModified = obj.UpdatedDateTimeOffset ?? DateTimeOffset.UtcNow
            };
        }
        catch (Google.GoogleApiException ex) when (ex.Error?.Code == 404)
        {
            return StorageErrors.FileNotFound(path: path);
        }
    }

    private async Task<IReadOnlyDictionary<int, string>?> UploadThumbnailsAsync(
        Stream original,
        UploadOptions options,
        string basePath,
        CancellationToken ct)
    {
        if (!options.GenerateThumbnails || options.ThumbnailWidths == null)
            return null;

        var dict = new Dictionary<int, string>();

        foreach (var w in options.ThumbnailWidths)
        {
            original.Position = 0;

            using var thumb =
                await ImageProcessingHelper.GenerateThumbnailAsync(
                    originalStream: original, targetWidth: w, quality: options.Quality, ct: ct);

            var thumbPath = basePath.Replace(oldValue: ".", newValue: $"_{w}.");

            await _client.UploadObjectAsync(
                bucket: _options.GoogleBucketName!,
                objectName: thumbPath,
                contentType: "image/webp",
                source: thumb,
                cancellationToken: ct);

            dict[key: w] = GetFileUrl(path: thumbPath);
        }

        return dict;
    }

    private string GetFileUrl(string path)
    {
        if (!string.IsNullOrEmpty(value: _options.GoogleBaseUrl))
            return $"{_options.GoogleBaseUrl.TrimEnd(trimChar: '/')}/{path}";

        return $"https://storage.googleapis.com/{_options.GoogleBucketName}/{path}";
    }

    private string ExtractPath(string fileUrl)
    {
        if (!string.IsNullOrEmpty(value: _options.GoogleBaseUrl))
            return fileUrl.Replace(oldValue: _options.GoogleBaseUrl, newValue: "").Trim(trimChar: '/');

        return fileUrl
            .Replace(oldValue: $"https://storage.googleapis.com/{_options.GoogleBucketName}/", newValue: "")
            .Trim(trimChar: '/');
    }
}
