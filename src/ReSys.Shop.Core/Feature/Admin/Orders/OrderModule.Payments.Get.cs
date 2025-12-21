using MapsterMapper;
using OrderPayments = ReSys.Shop.Core.Domain.Orders.Payments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Payments
    {
        public static class GetList
        {
            public sealed record Result : Models.PaymentItem;
            public sealed record Query(Guid OrderId) : IQuery<List<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, List<Result>>
            {
                public async Task<ErrorOr<List<Result>>> Handle(Query request, CancellationToken ct)
                {
                    var payments = await dbContext.Set<OrderPayments.Payment>()
                        .Where(p => p.OrderId == request.OrderId)
                        .AsNoTracking()
                        .ToListAsync(ct);

                    return mapper.Map<List<Result>>(payments);
                }
            }
        }
    }
}