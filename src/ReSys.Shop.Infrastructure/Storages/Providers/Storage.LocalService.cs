using FluentStorage;
using FluentStorage.Blobs;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using ReSys.Shop.Core.Common.Services.Storage.Models;
using ReSys.Shop.Core.Common.Services.Storage.Services;
using ReSys.Shop.Infrastructure.Storages.Helpers;
using ReSys.Shop.Infrastructure.Storages.Options;

using Serilog;

namespace ReSys.Shop.Infrastructure.Storages.Providers;

public sealed class LocalStorageService : IStorageService
{
    private readonly IBlobStorage _storage;
    private readonly StorageOptions _options;

    public LocalStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;

        try
        {
            Directory.CreateDirectory(path: _options.LocalPath);
            _storage = StorageFactory.Blobs.DirectoryFiles(directoryFullName: _options.LocalPath);
        }
        catch (Exception ex)
        {
            Log.Error(exception: ex, messageTemplate: "Failed to initialize local storage");
            throw;
        }
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
                    await ImageProcessingHelper.ProcessImageAsync(
                        originalStream: input, options: options, ct: cancellationToken);

                uploadStream = processed.ProcessedStream;
                width = processed.Width;
                height = processed.Height;
                contentType = processed.ContentType;
            }

            await _storage.WriteAsync(
                fullPath: path,
                dataStream: uploadStream,
                append: options.Overwrite,
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
            Log.Error(exception: ex, messageTemplate: "Local upload failed");
            return StorageErrors.UploadFailed(reason: ex.Message);
        }
    }

    public async Task<ErrorOr<IReadOnlyList<StorageFileInfo>>> UploadBatchAsync(
        IEnumerable<IFormFile> files,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var list = new List<StorageFileInfo>();

        foreach (var file in files)
        {
            var result = await UploadFileAsync(file: file, options: options, cancellationToken: cancellationToken);
            if (result.IsError)
                return result.Errors;

            list.Add(item: result.Value);
        }

        return list.AsReadOnly();
    }

    public async Task<ErrorOr<Stream>> GetFileStreamAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = GetBlobPath(url: fileUrl);

        if (!await _storage.ExistsAsync(fullPath: path, cancellationToken: cancellationToken))
            return StorageErrors.FileNotFound(path: path);

        var ms = new MemoryStream();
        await _storage.ReadToStreamAsync(fullPath: path, targetStream: ms, cancellationToken: cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task<ErrorOr<Success>> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var path = GetBlobPath(url: fileUrl);

        if (!await _storage.ExistsAsync(fullPath: path, cancellationToken: cancellationToken))
            return StorageErrors.FileNotFound(path: path);

        await _storage.DeleteAsync(fullPath: path, cancellationToken: cancellationToken);
        return Result.Success;
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

    public async Task<ErrorOr<bool>> ExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        return await _storage.ExistsAsync(
            fullPath: GetBlobPath(url: fileUrl),
            cancellationToken: cancellationToken);
    }

    public async Task<ErrorOr<IReadOnlyList<StorageFileMetadata>>> ListFilesAsync(
        string? prefix = null,
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        var blobs = await _storage.ListAsync(
            folderPath: prefix,
            recurse: recursive);

        return blobs
            .Where(predicate: b => !b.IsFolder)
            .Select(selector: b => new StorageFileMetadata
            {
                Path = b.FullPath,
                Url = GetFileUrl(path: b.FullPath),
                Length = b.Size ?? 0,
                LastModified = b.LastModificationTime.GetValueOrDefault()
            })
            .ToList()
            .AsReadOnly();
    }

    public async Task<ErrorOr<StorageFileInfo>> CopyFileAsync(
        string sourceUrl,
        string destinationPath,
        bool overwrite,
        CancellationToken cancellationToken = default)
    {
        var src = GetBlobPath(url: sourceUrl);

        if (!await _storage.ExistsAsync(fullPath: src, cancellationToken: cancellationToken))
            return StorageErrors.FileNotFound(path: src);

        await using var ms = new MemoryStream();
        await _storage.ReadToStreamAsync(fullPath: src, targetStream: ms, cancellationToken: cancellationToken);
        ms.Position = 0;

        await _storage.WriteAsync(
            fullPath: destinationPath,
            dataStream: ms,
            append: overwrite,
            cancellationToken: cancellationToken);

        return new StorageFileInfo
        {
            Path = destinationPath,
            Url = GetFileUrl(path: destinationPath),
            ContentType = "application/octet-stream",
            Length = ms.Length,
            LastModified = DateTimeOffset.UtcNow
        };
    }

    public async Task<ErrorOr<StorageFileInfo>> MoveFileAsync(
        string sourceUrl,
        string destinationPath,
        bool overwrite,
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

    public Task<ErrorOr<StorageFileInfo>> GetMetadataAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ErrorOr<StorageFileInfo>>(
            result: StorageErrors.OperationFailed(operation: "Metadata", reason: "Not supported"));
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

            await _storage.WriteAsync(
                fullPath: thumbPath,
                dataStream: thumb,
                append: options.Overwrite,
                cancellationToken: ct);

            dict[key: w] = GetFileUrl(path: thumbPath);
        }

        return dict;
    }

    private string GetFileUrl(string path)
        => $"{_options.BaseUrl.TrimEnd(trimChar: '/')}/{path}";

    private string GetBlobPath(string url)
    {
        return url
            .Replace(oldValue: _options.BaseUrl, newValue: "", comparisonType: StringComparison.OrdinalIgnoreCase)
            .Trim(trimChar: '/');
    }
}
