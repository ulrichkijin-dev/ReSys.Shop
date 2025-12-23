using Mapster;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Shop.Core.Feature.Storefront.Taxons;

public static partial class TaxonModule
{
    public static class Models
    {
        public record TaxonItem
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Presentation { get; init; } = string.Empty;
            public string Permalink { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string? PrettyName { get; init; }
            public int Position { get; init; }
            public int Depth { get; init; }
            public Guid TaxonomyId { get; init; }
            public Guid? ParentId { get; init; }
            public List<TaxonItem> Children { get; init; } = [];
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Taxon, TaxonItem>()
                    .Map(dest => dest.Children, src => src.Children);
            }
        }
    }
}
