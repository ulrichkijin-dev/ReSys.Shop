using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Storefront.Orders;

public static partial class OrderModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed record Query(QueryableParams Params) : IQuery<PaginationList<Models.OrderItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : IQueryHandler<Query, PaginationList<Models.OrderItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.OrderItem>>> Handle(Query request, CancellationToken ct)
                {
                    var userId = userContext.UserId;
                    var query = dbContext.Set<Order>()
                        .Where(o => o.UserId == userId && o.State != Order.OrderState.Cart)
                        .AsNoTracking();

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<Order>)query).OrderByDescending(o => o.CreatedAt);
                    }

                    return await query
                        .ProjectToType<Models.OrderItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class ByNumber
        {
            public sealed record Query(string Number) : IQuery<Models.OrderDetail>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : IQueryHandler<Query, Models.OrderDetail>
            {
                public async Task<ErrorOr<Models.OrderDetail>> Handle(Query request, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Where(o => o.Number == request.Number && o.UserId == userContext.UserId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (order == null) return Order.Errors.NotFound(Guid.Empty); // Generic not found for storefront

                    return mapper.Map<Models.OrderDetail>(order);
                }
            }
        }

        public static class ByToken
        {
            public sealed record Query(string Token) : IQuery<Models.OrderDetail>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.OrderDetail>
            {
                public async Task<ErrorOr<Models.OrderDetail>> Handle(Query request, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.Shipments)
                        .Include(o => o.Payments)
                        .Where(o => o.AdhocCustomerId == request.Token)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (order == null) return Order.Errors.NotFound(Guid.Empty);

                    return mapper.Map<Models.OrderDetail>(order);
                }
            }
        }

        public static class Status
        {
            public sealed record Query(string Number) : IQuery<Models.OrderStatus>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : IQueryHandler<Query, Models.OrderStatus>
            {
                public async Task<ErrorOr<Models.OrderStatus>> Handle(Query request, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Where(o => o.Number == request.Number && o.UserId == userContext.UserId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (order == null) return Order.Errors.NotFound(Guid.Empty);

                    return mapper.Map<Models.OrderStatus>(order);
                }
            }
        }
    }
}
