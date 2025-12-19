using Microsoft.Extensions.Options;

namespace ReSys.Shop.Infrastructure.Storages.Options;

public enum StorageProvider
{
    Local,
    Azure,
    GoogleCloud
}

public sealed class StorageOptions : IValidateOptions<StorageOptions>
{
    public const string Section = "Storage";

    public StorageProvider Provider { get; set; } = StorageProvider.Local;

    public string LocalPath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "/uploads";

    public string? AzureConnectionString { get; set; }
    public string? AzureContainerName { get; set; }
    public string? AzureCdnUrl { get; set; }

    public string? GoogleProjectId { get; set; }
    public string? GoogleBucketName { get; set; }

    /// <summary>
    /// Optional path to service-account JSON file.
    /// If null, GOOGLE_APPLICATION_CREDENTIALS will be used.
    /// </summary>
    public string? GoogleCredentialsPath { get; set; }

    /// <summary>
    /// Optional CDN or public base URL.
    /// Example: https://storage.googleapis.com/my-bucket
    /// </summary>
    public string? GoogleBaseUrl { get; set; }

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx"
    ];

    public int UploadTimeoutSeconds { get; set; } = 300;
    public int MaxConcurrentUploads { get; set; } = 10;

    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        var failures = new List<string>();

        if (options.MaxFileSizeBytes <= 0)
            failures.Add(item: "MaxFileSizeBytes must be greater than 0.");

        if (options.AllowedExtensions is not { Length: > 0 })
            failures.Add(item: "At least one AllowedExtension must be provided.");

        foreach (var ext in options.AllowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(value: ext) || !ext.StartsWith(value: "."))
                failures.Add(item: $"Invalid extension '{ext}'.");
        }

        switch (options.Provider)
        {
            case StorageProvider.Local:
                if (string.IsNullOrWhiteSpace(value: options.LocalPath))
                    failures.Add(item: "LocalPath is required for Local provider.");
                if (string.IsNullOrWhiteSpace(value: options.BaseUrl))
                    failures.Add(item: "BaseUrl is required for Local provider.");
                break;

            case StorageProvider.Azure:
                if (string.IsNullOrWhiteSpace(value: options.AzureConnectionString))
                    failures.Add(item: "AzureConnectionString is required.");
                if (string.IsNullOrWhiteSpace(value: options.AzureContainerName))
                    failures.Add(item: "AzureContainerName is required.");
                break;

            case StorageProvider.GoogleCloud:
                if (string.IsNullOrWhiteSpace(value: options.GoogleBucketName))
                    failures.Add(item: "GoogleBucketName is required.");
                if (string.IsNullOrWhiteSpace(value: options.GoogleProjectId))
                    failures.Add(item: "GoogleProjectId is required.");
                break;
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
