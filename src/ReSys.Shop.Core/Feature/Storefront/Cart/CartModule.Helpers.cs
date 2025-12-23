using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    private static async Task<Order?> GetCartAsync(IApplicationDbContext dbContext, IUserContext userContext, string? adhocCustomerId, CancellationToken ct)
    {
        var userId = userContext.UserId;
        
        var query = dbContext.Set<Order>()
            .Include(o => o.LineItems)
            .Include(o => o.OrderAdjustments)
            .Include(o => o.ShipAddress)
            .Include(o => o.BillAddress)
            .Include(o => o.Shipments)
            .Include(o => o.Payments)
            .Where(o => o.State == Order.OrderState.Cart);

        if (userId != null)
        {
            return await query.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync(ct);
        }

        if (adhocCustomerId != null)
        {
            return await query.Where(o => o.AdhocCustomerId == adhocCustomerId).FirstOrDefaultAsync(ct);
        }

        return null;
    }
}
