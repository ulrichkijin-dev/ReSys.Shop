using Mapster;

using MapsterMapper;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.Prices;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Variants;

public static partial class VariantModule
{
    public static partial class Prices
    {
        public static class Get
        {
            public sealed class Request : QueryableParams;
            public sealed record Result : Models.PriceItem;
            public sealed record Query(Guid VariantId, Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query request, CancellationToken ct)
                {
                    var prices = await applicationDbContext.Set<Price>()
                        .Where(predicate: p => p.VariantId == request.VariantId)
                        .AsQueryable()
                        .AsNoTracking()
                        .ApplySearch(searchParams: request.Request)
                        .ApplyFilters(filterParams: request.Request)
                        .ApplySort(sortParams: request.Request)
                        .ProjectToType<Result>(config: mapper.Config)
                        .ToPagedListOrAllAsync(pagingParams: request.Request, cancellationToken: ct);

                    return prices;
                }
            }
        }
    }
}