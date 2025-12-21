using MapsterMapper;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class GetList
        {
            public sealed record Result : Models.ShipmentItem;
            public sealed record Query(Guid OrderId) : IQuery<List<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, List<Result>>
            {
                public async Task<ErrorOr<List<Result>>> Handle(Query request, CancellationToken ct)
                {
                    var shipments = await dbContext.Set<Shipment>()
                        .Where(s => s.OrderId == request.OrderId)
                        .Include(s => s.StockLocation)
                        .AsNoTracking()
                        .ToListAsync(ct);

                    return mapper.Map<List<Result>>(shipments);
                }
            }
        }

        public static class GetById
        {
            public sealed record Result : Models.ShipmentItem;
            public sealed record Query(Guid OrderId, Guid ShipmentId) : IQuery<Result>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Query request, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>()
                        .Where(s => s.Id == request.ShipmentId && s.OrderId == request.OrderId)
                        .Include(s => s.StockLocation)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (shipment == null) return Shipment.Errors.NotFound(request.ShipmentId);

                    return mapper.Map<Result>(shipment);
                }
            }
        }
    }
}
