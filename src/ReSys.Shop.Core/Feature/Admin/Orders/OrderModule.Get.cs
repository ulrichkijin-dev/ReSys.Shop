using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed class Request : QueryableParams
            {
                public string? State { get; init; }
                public string? Email { get; init; }
            }

            public sealed record Result : Models.ListItem;
            public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query command, CancellationToken ct)
                {
                    var query = dbContext.Set<Order>()
                        .Include(o => o.User)
                        .AsNoTracking();

                    if (!string.IsNullOrEmpty(command.Request.State) && Enum.TryParse<Order.OrderState>(command.Request.State, out var state))
                    {
                        query = query.Where(o => o.State == state);
                    }

                    if (!string.IsNullOrEmpty(command.Request.Email))
                    {
                        query = query.Where(o => o.Email != null && o.Email.Contains(command.Request.Email));
                    }

                    var pagedResult = await query
                        .ApplySearch(command.Request)
                        .ApplyFilters(command.Request)
                        .ApplySort(command.Request)
                        .ProjectToType<Result>(mapper.Config)
                        .ToPagedListAsync(command.Request, ct);

                    return pagedResult;
                }
            }
        }

        public static class ById
        {
            public sealed record Result : Models.Detail;
            public sealed record Query(Guid Id) : IQuery<Result>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Query request, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.User)
                        .Include(o => o.LineItems)
                        .Include(o => o.OrderAdjustments)
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.StockLocation)
                        .Include(o => o.Payments)
                        .Include(o => o.Histories)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

                    if (order == null)
                        return Order.Errors.NotFound(request.Id);

                    return mapper.Map<Result>(order);
                }
            }
        }
    }
}