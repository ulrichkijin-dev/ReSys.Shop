namespace ReSys.Shop.Core.Common.Services.Storage.Models;

public static class StorageErrors
{
    public static Error FileNotFound(string path) =>
        Error.NotFound(code: "Storage.FileNotFound", description: $"File not found: {path}");

    public static Error FileAlreadyExists(string path) =>
        Error.Conflict(code: "Storage.FileAlreadyExists", description: $"File already exists: {path}");

    public static Error UploadFailed(string reason) =>
        Error.Failure(code: "Storage.UploadFailed", description: $"File upload failed: {reason}");

    public static Error DeleteFailed(string reason) =>
        Error.Failure(code: "Storage.DeleteFailed", description: $"File deletion failed: {reason}");

    public static Error ReadFailed(string reason) =>
        Error.Failure(code: "Storage.ReadFailed", description: $"File read failed: {reason}");

    public static Error CopyFailed(string reason) =>
        Error.Failure(code: "Storage.CopyFailed", description: $"File copy failed: {reason}");

    public static Error MoveFailed(string reason) =>
        Error.Failure(code: "Storage.MoveFailed", description: $"File move failed: {reason}");

    public static Error FileEmpty =>
        Error.Validation(code: "File.Empty", description: "No file was provided");

    public static Error FileEmptyContent =>
        Error.Validation(code: "File.EmptyContent", description: "The file has no content");

    public static Error FileTooLarge(long maxSizeBytes) =>
        Error.Validation(code: "File.TooLarge",
            description: $"File exceeds maximum size of {maxSizeBytes / 1024 / 1024}MB");

    public static Error FileInvalidType(string extension, IEnumerable<string> allowed) =>
        Error.Validation(code: "File.InvalidType",
            description: $"File type '{extension}' not allowed. Allowed: {string.Join(separator: ", ", values: allowed)}");

    public static Error InvalidUrl =>
        Error.Validation(code: "File.InvalidUrl", description: "File URL is invalid or empty");

    public static Error InvalidFileName =>
        Error.Validation(code: "File.InvalidFileName", description: "File name contains invalid characters");

    public static Error ImageProcessingFailed(string reason) =>
        Error.Failure(code: "Image.ProcessingFailed", description: $"Image processing failed: {reason}");

    public static Error OperationFailed(string operation, string reason) =>
        Error.Failure(code: $"Storage.{operation}Failed", description: reason);
}