using Mapster;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.LineItems;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Adjustments;
using ReSys.Shop.Core.Domain.Orders.History;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Models
    {
        public record ListItem
        {
            public Guid Id { get; init; }
            public string Number { get; init; } = string.Empty;
            public string State { get; init; } = string.Empty;
            public string? Email { get; init; }
            public decimal Total { get; init; }
            public string Currency { get; init; } = string.Empty;
            public int ItemCount { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
            public string? UserName { get; init; }
        }

        public record Detail : ListItem
        {
            public decimal ItemTotal { get; init; }
            public decimal ShipmentTotal { get; init; }
            public decimal AdjustmentTotal { get; init; }
            public string? SpecialInstructions { get; init; }
            public DateTimeOffset? CompletedAt { get; init; }
            public DateTimeOffset? CanceledAt { get; init; }
            public List<LineItemItem> LineItems { get; init; } = [];
            public List<AdjustmentItem> Adjustments { get; init; } = [];
            public List<ShipmentItem> Shipments { get; init; } = [];
            public List<PaymentItem> Payments { get; init; } = [];
            public List<HistoryItem> Histories { get; init; } = [];
        }

        public record LineItemItem
        {
            public Guid Id { get; init; }
            public Guid VariantId { get; init; }
            public string CapturedName { get; init; } = string.Empty;
            public string? CapturedSku { get; init; }
            public int Quantity { get; init; }
            public decimal UnitPrice { get; init; }
            public decimal Total { get; init; }
            public bool IsPromotional { get; init; }
        }

        public record AdjustmentItem
        {
            public Guid Id { get; init; }
            public string Description { get; init; } = string.Empty;
            public decimal Amount { get; init; }
            public string Scope { get; init; } = string.Empty;
            public bool Eligible { get; init; }
            public bool IsPromotion { get; init; }
        }

        public record ShipmentItem
        {
            public Guid Id { get; init; }
            public string Number { get; init; } = string.Empty;
            public string State { get; init; } = string.Empty;
            public string? TrackingNumber { get; init; }
            public string StockLocationName { get; init; } = string.Empty;
            public DateTimeOffset? ShippedAt { get; init; }
        }

        public record PaymentItem
        {
            public Guid Id { get; init; }
            public string State { get; init; } = string.Empty;
            public decimal Amount { get; init; }
            public string PaymentMethodType { get; init; } = string.Empty;
            public string? ReferenceTransactionId { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
        }

        public record HistoryItem
        {
            public Guid Id { get; init; }
            public string Description { get; init; } = string.Empty;
            public string? FromState { get; init; }
            public string ToState { get; init; } = string.Empty;
            public string? TriggeredBy { get; init; }
            public DateTimeOffset CreatedAt { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Order, ListItem>()
                    .Map(dest => dest.State, src => src.State.ToString())
                    .Map(dest => dest.UserName, src => src.User != null ? src.User.UserName : null);

                config.NewConfig<Order, Detail>()
                    .Inherits<Order, ListItem>()
                    .Map(dest => dest.LineItems, src => src.LineItems)
                    .Map(dest => dest.Adjustments, src => src.OrderAdjustments)
                    .Map(dest => dest.Shipments, src => src.Shipments)
                    .Map(dest => dest.Payments, src => src.Payments)
                    .Map(dest => dest.Histories, src => src.Histories);

                config.NewConfig<LineItem, LineItemItem>();
                
                config.NewConfig<OrderAdjustment, AdjustmentItem>()
                    .Map(dest => dest.Amount, src => src.AmountCents / 100m)
                    .Map(dest => dest.Scope, src => src.Scope.ToString());

                config.NewConfig<Shipment, ShipmentItem>()
                    .Map(dest => dest.State, src => src.State.ToString())
                    .Map(dest => dest.StockLocationName, src => src.StockLocation != null ? src.StockLocation.Name : string.Empty);

                config.NewConfig<Payment, PaymentItem>()
                    .Map(dest => dest.State, src => src.State.ToString());

                config.NewConfig<OrderHistory, HistoryItem>()
                    .Map(dest => dest.FromState, src => src.FromState.HasValue ? src.FromState.Value.ToString() : null)
                    .Map(dest => dest.ToState, src => src.ToState.ToString());
            }
        }
    }
}
