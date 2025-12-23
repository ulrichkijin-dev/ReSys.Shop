using Mapster;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;
using ReSys.Shop.Core.Feature.Storefront.Taxons;

namespace ReSys.Shop.Core.Feature.Storefront.Taxonomies;

public static partial class TaxonomyModule
{
    public static class Models
    {
        public record TaxonomyItem
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Presentation { get; init; } = string.Empty;
            public int Position { get; init; }
            public TaxonModule.Models.TaxonItem? Root { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Taxonomy, TaxonomyItem>()
                    .Map(dest => dest.Root, src => src.Root);
            }
        }
    }
}
