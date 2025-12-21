using ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Inventories.StockItems;

public static class StockReservations
{
    public sealed class OnShipmentCreated(IApplicationDbContext dbContext) : IDomainEventHandler<Order.Events.ShipmentCreated>
    {
        public async Task Handle(Order.Events.ShipmentCreated notification, CancellationToken ct)
        {
            var shipment = await dbContext.Set<Shipment>()
                .Include(s => s.InventoryUnits)
                .FirstOrDefaultAsync(s => s.Id == notification.ShipmentId, ct);

            if (shipment == null) return;

            var variantsToReserve = shipment.InventoryUnits
                .GroupBy(u => u.VariantId);

            foreach (var group in variantsToReserve)
            {
                var variantId = group.Key;
                
                var totalQuantity = await dbContext.Set<Shipment>()
                    .Where(s => s.OrderId == notification.OrderId && s.StockLocationId == shipment.StockLocationId)
                    .SelectMany(s => s.InventoryUnits)
                    .CountAsync(u => u.VariantId == variantId, ct);

                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.StockLocationId == shipment.StockLocationId && si.VariantId == variantId, ct);

                if (stockItem != null)
                {
                    stockItem.Reserve(totalQuantity, notification.OrderId);
                }
            }
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public sealed class OnShipmentItemUpdated(IApplicationDbContext dbContext) : IDomainEventHandler<Order.Events.ShipmentItemUpdated>
    {
        public async Task Handle(Order.Events.ShipmentItemUpdated notification, CancellationToken ct)
        {
            var shipment = await dbContext.Set<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == notification.ShipmentId, ct);

            if (shipment == null) return;

            var totalQuantity = await dbContext.Set<Shipment>()
                .Where(s => s.OrderId == notification.OrderId && s.StockLocationId == shipment.StockLocationId)
                .SelectMany(s => s.InventoryUnits)
                .CountAsync(u => u.VariantId == notification.VariantId, ct);

            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.StockLocationId == shipment.StockLocationId && si.VariantId == notification.VariantId, ct);

            if (stockItem != null)
            {
                stockItem.Reserve(totalQuantity, notification.OrderId);
            }
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
