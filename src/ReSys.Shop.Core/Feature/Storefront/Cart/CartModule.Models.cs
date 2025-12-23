using Mapster;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.LineItems;
using ReSys.Shop.Core.Domain.Orders.Adjustments;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Models
    {
        public record CartDetail
        {
            public Guid Id { get; init; }
            public string Number { get; init; } = string.Empty;
            public string? AdhocCustomerId { get; init; }
            public string State { get; init; } = string.Empty;
            public decimal Total { get; init; }
            public decimal ItemTotal { get; init; }
            public decimal ShipmentTotal { get; init; }
            public decimal AdjustmentTotal { get; init; }
            public string Currency { get; init; } = string.Empty;
            public string? Email { get; init; }
            public string? PaymentClientSecret { get; set; }
            public string? PaymentApprovalUrl { get; set; }
            public List<CartLineItem> LineItems { get; init; } = [];
            public List<CartAdjustment> Adjustments { get; init; } = [];
            public CartAddress? ShippingAddress { get; init; }
            public CartAddress? BillingAddress { get; init; }
        }

        public record CartLineItem
        {
            public Guid Id { get; init; }
            public Guid VariantId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string? Sku { get; init; }
            public int Quantity { get; init; }
            public decimal Price { get; init; }
            public decimal Total { get; init; }
        }

        public record CartAdjustment
        {
            public string Description { get; init; } = string.Empty;
            public decimal Amount { get; init; }
            public string Scope { get; init; } = string.Empty;
        }

        public record CartAddress
        {
            public string FirstName { get; init; } = string.Empty;
            public string LastName { get; init; } = string.Empty;
            public string Address1 { get; init; } = string.Empty;
            public string? Address2 { get; init; }
            public string City { get; init; } = string.Empty;
            public string ZipCode { get; init; } = string.Empty;
            public string Phone { get; init; } = string.Empty;
            public string CountryName { get; init; } = string.Empty;
            public string? StateName { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<Order, CartDetail>()
                    .Map(dest => dest.AdhocCustomerId, src => src.AdhocCustomerId)
                    .Map(dest => dest.State, src => src.State.ToString())
                    .Map(dest => dest.LineItems, src => src.LineItems)
                    .Map(dest => dest.Adjustments, src => src.OrderAdjustments)
                    .Map(dest => dest.ShippingAddress, src => src.ShipAddress)
                    .Map(dest => dest.BillingAddress, src => src.BillAddress)
                    .AfterMapping((src, dest) =>
                    {
                        var latestPayment = src.Payments
                            .OrderByDescending(p => p.CreatedAt)
                            .FirstOrDefault(p => p.State == Domain.Orders.Payments.Payment.PaymentState.Pending 
                                              || p.State == Domain.Orders.Payments.Payment.PaymentState.RequiresAction
                                              || p.State == Domain.Orders.Payments.Payment.PaymentState.Authorized);

                        if (latestPayment?.PublicMetadata != null)
                        {
                            if (latestPayment.PublicMetadata.TryGetValue("client_secret", out var secret))
                                dest.PaymentClientSecret = secret?.ToString();
                            
                            if (latestPayment.PublicMetadata.TryGetValue("approval_url", out var url))
                                dest.PaymentApprovalUrl = url?.ToString();
                        }
                    });

                config.NewConfig<LineItem, CartLineItem>()
                    .Map(dest => dest.Name, src => src.CapturedName)
                    .Map(dest => dest.Sku, src => src.CapturedSku)
                    .Map(dest => dest.Price, src => src.PriceCents / 100m);

                config.NewConfig<OrderAdjustment, CartAdjustment>()
                    .Map(dest => dest.Amount, src => src.AmountCents / 100m)
                    .Map(dest => dest.Scope, src => src.Scope.ToString());
            }
        }
    }
}
