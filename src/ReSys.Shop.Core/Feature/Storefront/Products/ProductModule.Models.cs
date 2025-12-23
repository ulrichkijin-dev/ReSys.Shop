using Mapster;
using ReSys.Shop.Core.Domain.Catalog.Products;

namespace ReSys.Shop.Core.Feature.Storefront.Products;

public static partial class ProductModule
{
    public static class Models
    {
        public record ProductItem
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Presentation { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string Slug { get; init; } = string.Empty;
            public string? MetaTitle { get; init; }
            public string? MetaDescription { get; init; }
            public bool IsDigital { get; init; }
            public string? Currency { get; set; }
            public decimal? Price { get; set; }
            public decimal? DisplayPrice { get; set; }
            public string? ImageUrl { get; set; }
        }

        public record ProductDetail : ProductItem
        {
            public List<VariantItem> Variants { get; init; } = [];
            public List<PropertyItem> Properties { get; init; } = [];
        }

        public record VariantItem
        {
            public Guid Id { get; init; }
            public string? Sku { get; init; }
            public decimal Price { get; init; }
            public bool IsMaster { get; init; }
            public bool InStock { get; init; }
        }

        public record PropertyItem
        {
            public string Name { get; init; } = string.Empty;
            public string Presentation { get; init; } = string.Empty;
            public string Value { get; init; } = string.Empty;
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Product, ProductItem>()
                    .Map(dest => dest.ImageUrl, src => src.DefaultImage != null ? src.DefaultImage.Url : null)
                    .AfterMapping((src, dest) =>
                    {
                        var master = src.Variants.FirstOrDefault(v => v.IsMaster);
                        if (master != null)
                        {
                            var price = master.Prices.FirstOrDefault(); // Simplified, should consider currency
                            if (price != null)
                            {
                                dest.Price = price.Amount;
                                dest.DisplayPrice = price.Amount;
                                dest.Currency = price.Currency;
                            }
                        }
                    });

                config.NewConfig<Product, ProductDetail>()
                    .Inherits<Product, ProductItem>()
                    .Map(dest => dest.Variants, src => src.Variants)
                    .Map(dest => dest.Properties, src => src.ProductPropertyTypes);

                config.NewConfig<Domain.Catalog.Products.Variants.Variant, VariantItem>()
                    .Map(dest => dest.Price, src => src.Prices.FirstOrDefault() != null ? src.Prices.FirstOrDefault()!.Amount : 0);

                config.NewConfig<Domain.Catalog.Products.PropertyTypes.ProductPropertyType, PropertyItem>()
                    .Map(dest => dest.Name, src => src.PropertyType.Name)
                    .Map(dest => dest.Presentation, src => src.PropertyType.Presentation)
                    .Map(dest => dest.Value, src => src.PropertyTypeValue);
            }
        }
    }
}
