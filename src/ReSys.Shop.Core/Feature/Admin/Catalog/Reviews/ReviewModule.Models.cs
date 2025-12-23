using Mapster;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;

namespace ReSys.Shop.Core.Feature.Admin.Catalog.Reviews;

public static partial class ReviewModule
{
    public static class Models
    {
        public record ReviewItem
        {
            public Guid Id { get; init; }
            public Guid ProductId { get; init; }
            public string? ProductName { get; init; }
            public string UserId { get; init; } = string.Empty;
            public string? UserName { get; init; }
            public int Rating { get; init; }
            public string? Title { get; init; }
            public string? Comment { get; init; }
            public string Status { get; init; } = string.Empty;
            public DateTimeOffset CreatedAt { get; init; }
            public string? ModeratedBy { get; set; }
            public DateTimeOffset? ModeratedAt { get; set; }
            public string? ModerationNotes { get; set; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Review, ReviewItem>()
                    .Map(dest => dest.Status, src => src.Status.ToString())
                    .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
                    .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : null);
            }
        }
    }
}
