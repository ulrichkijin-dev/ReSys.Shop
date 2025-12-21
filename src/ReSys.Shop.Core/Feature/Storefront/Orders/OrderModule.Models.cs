using Mapster;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Storefront.Orders;

public static partial class OrderModule
{
    public static class Models
    {
        public record OrderItem
        {
            public Guid Id { get; init; }
            public string Number { get; init; } = string.Empty;
            public string State { get; init; } = string.Empty;
            public decimal Total { get; init; }
            public string Currency { get; init; } = string.Empty;
            public DateTimeOffset CreatedAt { get; init; }
        }

        public record OrderDetail : OrderItem
        {
            public List<OrderLineItem> LineItems { get; init; } = [];
            public string? ShipmentState { get; init; }
            public string? PaymentState { get; init; }
        }

        public record OrderLineItem
        {
            public string Name { get; init; } = string.Empty;
            public int Quantity { get; init; }
            public decimal Price { get; init; }
        }

        public record OrderStatus
        {
            public string Number { get; init; } = string.Empty;
            public string State { get; init; } = string.Empty;
            public DateTimeOffset? CompletedAt { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Order, OrderItem>()
                    .Map(dest => dest.State, src => src.State.ToString());

                config.NewConfig<Order, OrderDetail>()
                    .Inherits<Order, OrderItem>()
                    .Map(dest => dest.LineItems, src => src.LineItems);

                config.NewConfig<Order, OrderStatus>()
                    .Map(dest => dest.State, src => src.State.ToString());
            }
        }
    }
}
