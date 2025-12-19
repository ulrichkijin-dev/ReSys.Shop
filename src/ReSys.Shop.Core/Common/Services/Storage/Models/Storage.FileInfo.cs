namespace ReSys.Shop.Core.Common.Services.Storage.Models;

/// <summary>
/// Detailed file information returned after upload or metadata query.
/// </summary>
public sealed record StorageFileInfo
{
    public required string Path { get; init; }
    public required string Url { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
    public required DateTimeOffset LastModified { get; init; }
    public string? ETag { get; init; }

    public int? Width { get; init; }
    public int? Height { get; init; }

    public IReadOnlyDictionary<int, string>? Thumbnails { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Lightweight metadata used in directory listings.
/// </summary>
public sealed record StorageFileMetadata
{
    public required string Path { get; init; }
    public required string Url { get; init; }
    public required long Length { get; init; }
    public required DateTimeOffset LastModified { get; init; }
    public string? ContentType { get; init; }
    public string? ETag { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public int? Width { get; init; }
    public int? Height { get; init; }
}