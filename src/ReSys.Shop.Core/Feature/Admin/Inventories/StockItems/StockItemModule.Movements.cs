using Mapster;

using MapsterMapper;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Inventories.Movements;


namespace  ReSys.Shop.Core.Feature.Admin.Inventories.StockItems;

public static partial class StockItemModule
{
    public static class Movements
    {
        public static class Get
        {
            public sealed class Request : QueryableParams;
            public sealed record Result : Models.MovementItem;
            public sealed record Query(Guid StockItemId, Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query query, CancellationToken ct)
                {
                    var movements = await applicationDbContext.Set<StockMovement>()
                        .Where(predicate: sm => sm.StockItemId == query.StockItemId)
                        .OrderByDescending(keySelector: sm => sm.CreatedAt)
                        .AsNoTracking()
                        .ApplySearch(searchParams: query.Request)
                        .ApplyFilters(filterParams: query.Request)
                        .ApplySort(sortParams: query.Request)
                        .ProjectToType<Result>(config: mapper.Config)
                        .ToPagedListAsync(pagingParams: query.Request, cancellationToken: ct);

                    return movements;
                }
            }
        }
    }
}