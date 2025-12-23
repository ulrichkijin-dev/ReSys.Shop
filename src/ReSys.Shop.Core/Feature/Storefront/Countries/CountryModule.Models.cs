using Mapster;
using ReSys.Shop.Core.Domain.Location.Countries;

namespace ReSys.Shop.Core.Feature.Storefront.Countries;

public static partial class CountryModule
{
    public static class Models
    {
        public record CountryItem
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Iso { get; init; } = string.Empty;
            public string Iso3 { get; init; } = string.Empty;
            public List<StateItem> States { get; init; } = [];
        }

        public record StateItem
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Abbreviation { get; init; } = string.Empty;
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Country, CountryItem>()
                    .Map(dest => dest.States, src => src.States);

                config.NewConfig<Domain.Location.States.State, StateItem>();
            }
        }
    }
}
