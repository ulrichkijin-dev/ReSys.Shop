using ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Inventories.StockItems;

public static class StockConfirmation
{
    public sealed class OnShipmentShipped(IApplicationDbContext dbContext) : IDomainEventHandler<Shipment.Events.Shipped>
    {
        public async Task Handle(Shipment.Events.Shipped notification, CancellationToken ct)
        {
             var shipment = await dbContext.Set<Shipment>()
                .Include(s => s.InventoryUnits)
                .FirstOrDefaultAsync(s => s.Id == notification.ShipmentId, ct);

            if (shipment == null) return;

            var variantsToShip = shipment.InventoryUnits
                .GroupBy(u => u.VariantId);

            foreach (var group in variantsToShip)
            {
                var variantId = group.Key;
                var quantity = group.Count();

                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.StockLocationId == shipment.StockLocationId && si.VariantId == variantId, ct);

                if (stockItem != null)
                {
                    stockItem.ConfirmShipment(quantity, notification.ShipmentId, notification.OrderId);
                }
            }
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
