using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Location.Countries;

namespace ReSys.Shop.Core.Feature.Storefront.Countries;

public static partial class CountryModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed record Query(QueryableParams Params) : IQuery<PaginationList<Models.CountryItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.CountryItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.CountryItem>>> Handle(Query request, CancellationToken ct)
                {
                    var query = dbContext.Set<Country>()
                        .AsNoTracking();

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<Country>)query).OrderBy(c => c.Name);
                    }

                    return await query
                        .ProjectToType<Models.CountryItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class ById
        {
            public sealed record Query(Guid Id) : IQuery<Models.CountryItem>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.CountryItem>
            {
                public async Task<ErrorOr<Models.CountryItem>> Handle(Query request, CancellationToken ct)
                {
                    var country = await dbContext.Set<Country>()
                        .Include(c => c.States)
                        .Where(c => c.Id == request.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (country == null) return Country.Errors.NotFound(request.Id);

                    return mapper.Map<Models.CountryItem>(country);
                }
            }
        }

        public static class Default
        {
            public sealed record Query() : IQuery<Models.CountryItem>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.CountryItem>
            {
                public async Task<ErrorOr<Models.CountryItem>> Handle(Query request, CancellationToken ct)
                {
                    // Logic to get default country - for now just the first one or by ISO "US"
                    var country = await dbContext.Set<Country>()
                        .Include(c => c.States)
                        .OrderBy(c => c.Iso == "US" ? 0 : 1)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (country == null) return Error.NotFound("Country.DefaultNotFound", "Default country not found.");

                    return mapper.Map<Models.CountryItem>(country);
                }
            }
        }
    }
}
