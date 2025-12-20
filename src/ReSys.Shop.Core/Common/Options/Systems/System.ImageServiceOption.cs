using System.ComponentModel.DataAnnotations;

namespace ReSys.Shop.Core.Common.Options.Systems;

/// <summary>
/// Configuration options for the Fashion Image Search (ML) service.
/// </summary>
public sealed class ImageServiceOption : SystemOptionBase
{
    public const string Section = "ImageService";

    public ImageServiceOption()
    {
        SystemName = "ReSys.ImageSearchService";
        DefaultPage = "/docs"; // Points to FastAPI swagger docs by default
    }

    /// <summary>
    /// API Key for authenticating requests to the Image Search Service.
    /// </summary>
    [Required, MinLength(16)]
    public string ApiKey { get; set; } = "default-secure-api-key-change-me";
}
