using Mapster;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;

namespace ReSys.Shop.Core.Feature.Storefront.Reviews;

public static partial class ReviewModule
{
    public static class Models
    {
        public record ReviewItem
        {
            public Guid Id { get; init; }
            public Guid ProductId { get; init; }
            public string UserId { get; init; } = string.Empty;
            public string? UserName { get; init; }
            public int Rating { get; init; }
            public string? Title { get; init; }
            public string? Comment { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
            public int HelpfulCount { get; init; }
            public int NotHelpfulCount { get; init; }
            public bool IsVerifiedPurchase { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Review, ReviewItem>()
                    .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : null);
            }
        }
    }
}
