using Quartz;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;
using System.Net.Http.Json;

using ReSys.Shop.Infrastructure.Persistence.Contexts;
using Serilog;

namespace ReSys.Shop.Infrastructure.Backgrounds.Jobs.Catalog;

[DisallowConcurrentExecution]
public sealed class ImageProcessingJob : IJob
{
    public static readonly JobKey JobKey = new("image-processing-job", "catalog");
    public static readonly TriggerKey TriggerKey = new("image-processing-trigger", "catalog");
    public const string Description = "Processes new or updated product images to generate embeddings via the Python service.";
    public const string CronExpression = "0/30 * * * * ?"; // Every 30 seconds

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger = Log.ForContext<ImageProcessingJob>();

    public ImageProcessingJob(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // Find images that are missing embeddings for any of the 3 thesis models
            // and are of type 'Search' (or 'Default' if we want to index everything, but Search is safer for now)
            var unprocessedImages = await _dbContext.Set<ProductImage>()
                .Where(pi => (pi.EmbeddingMobilenet == null || pi.EmbeddingEfficientnet == null || pi.EmbeddingClip == null) 
                             && pi.Type == nameof(ProductImage.ProductImageType.Search))
                .OrderBy(pi => pi.CreatedAt)
                .Take(50) // Batch size
                .ToListAsync(context.CancellationToken);

            if (!unprocessedImages.Any())
            {
                return;
            }

            _logger.Information("Found {Count} images requiring embedding generation.", unprocessedImages.Count);

            var client = _httpClientFactory.CreateClient("ImageSearchService"); 
            // Note: Ensure "ImageSearchService" is registered in DI with the correct BaseAddress (e.g., http://localhost:8000)

            foreach (var image in unprocessedImages)
            {
                try
                {
                    // Call Python API for each missing model
                    // Assuming the Python API has an endpoint like /extract/by-id/{id}?model={model}
                    // Or a batch endpoint. For simplicity, we loop here or use the batch if available.
                    
                    // 1. MobileNet
                    if (image.EmbeddingMobilenet == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=mobilenet_v3", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("mobilenet_v3", response.Embedding);
                        }
                    }

                    // 2. EfficientNet
                    if (image.EmbeddingEfficientnet == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=efficientnet_b0", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("efficientnet_b0", response.Embedding);
                        }
                    }

                    // 3. CLIP
                    if (image.EmbeddingClip == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=clip", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            if (response.Embedding.Length != 512)
                            {
                                throw new InvalidOperationException("Unexpected CLIP embedding size");
                            }
                            image.SetEmbedding("clip", response.Embedding);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to process image {ImageId}", image.Id);
                }
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
            _logger.Information("Successfully processed and saved embeddings for batch.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing ImageProcessingJob");
        }
    }

    // Helper DTO for deserialization
    public record EmbeddingResponse(string ImageId, string Model, float[] Embedding);
}
