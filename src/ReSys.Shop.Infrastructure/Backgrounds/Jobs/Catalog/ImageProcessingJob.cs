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
            // Find images that are missing embeddings for any of the 4 strategic models
            var unprocessedImages = await _dbContext.Set<ProductImage>()
                .Where(pi => (pi.EmbeddingEfficientnet == null || 
                              pi.EmbeddingConvnext == null || 
                              pi.EmbeddingFclip == null || 
                              pi.EmbeddingDino == null) 
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

            foreach (var image in unprocessedImages)
            {
                try
                {
                    // 1. EfficientNet B0
                    if (image.EmbeddingEfficientnet == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=efficientnet_b0", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("efficientnet_b0", response.Embedding);
                        }
                    }

                    // 2. ConvNeXt Tiny
                    if (image.EmbeddingConvnext == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=convnext_tiny", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("convnext_tiny", response.Embedding);
                        }
                    }

                    // 3. Fashion-CLIP
                    if (image.EmbeddingFclip == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=fashion_clip", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("fashion_clip", response.Embedding);
                        }
                    }

                    // 4. DINO ViT-S/16
                    if (image.EmbeddingDino == null)
                    {
                        var response = await client.GetFromJsonAsync<EmbeddingResponse>($"extract/by-id/{image.Id}?model=dino_vits16", context.CancellationToken);
                        if (response?.Embedding != null)
                        {
                            image.SetEmbedding("dino_vits16", response.Embedding);
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
