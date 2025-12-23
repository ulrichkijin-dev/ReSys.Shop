using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Cart",
            Tags = ["Storefront Cart"],
            Summary = "Shopping Cart API",
            Description = "Endpoints for managing the shopping cart and checkout process"
        };

        public static ApiEndpointMeta Get => new()
        {
            Name = "Storefront.Cart.Get",
            Summary = "Retrieve a cart",
            Description = "Returns the current shopping cart for the user or session.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Create => new()
        {
            Name = "Storefront.Cart.Create",
            Summary = "Create a cart",
            Description = "Creates a new shopping cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status201Created
        };

        public static ApiEndpointMeta Delete => new()
        {
            Name = "Storefront.Cart.Delete",
            Summary = "Delete a cart",
            Description = "Deletes the current shopping cart.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta AddItem => new()
        {
            Name = "Storefront.Cart.AddItem",
            Summary = "Add item to cart",
            Description = "Adds a product variant to the shopping cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta SetQuantity => new()
        {
            Name = "Storefront.Cart.SetQuantity",
            Summary = "Set item quantity",
            Description = "Updates the quantity of a specific line item in the cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta RemoveItem => new()
        {
            Name = "Storefront.Cart.RemoveItem",
            Summary = "Remove item from cart",
            Description = "Removes a specific line item from the shopping cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Empty => new()
        {
            Name = "Storefront.Cart.Empty",
            Summary = "Empty the cart",
            Description = "Removes all items from the shopping cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta ApplyCoupon => new()
        {
            Name = "Storefront.Cart.ApplyCoupon",
            Summary = "Apply coupon code",
            Description = "Applies a promotional coupon code to the cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta RemoveCoupon => new()
        {
            Name = "Storefront.Cart.RemoveCoupon",
            Summary = "Remove coupon",
            Description = "Removes an applied coupon from the cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Associate => new()
        {
            Name = "Storefront.Cart.Associate",
            Summary = "Associate cart with user",
            Description = "Links a guest cart to a registered user account.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta SetShippingAddress => new()
        {
            Name = "Storefront.Cart.SetShippingAddress",
            Summary = "Set shipping address",
            Description = "Sets the shipping address for the current cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta ChangeCurrency => new()
        {
            Name = "Storefront.Cart.ChangeCurrency",
            Summary = "Change cart currency",
            Description = "Updates the currency used for the shopping cart.",
            ResponseType = typeof(ApiResponse<Models.CartDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static class Checkout
        {
            public static ApiEndpointMeta Get => new()
            {
                Name = "Storefront.Checkout.Get",
                Summary = "Get checkout summary",
                Description = "Retrieves the current state of the checkout, including line items and payment intent details.",
                ResponseType = typeof(ApiResponse<CartModule.Checkout.GetSummary.Result>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Update => new()
            {
                Name = "Storefront.Checkout.Update",
                Summary = "Update checkout",
                Description = "Updates checkout details like shipping/billing addresses.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Next => new()
            {
                Name = "Storefront.Checkout.Next",
                Summary = "Next checkout step",
                Description = "Progresses the checkout to the next state.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Advance => new()
            {
                Name = "Storefront.Checkout.Advance",
                Summary = "Advance checkout",
                Description = "Attempts to advance the checkout as far as possible.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Complete => new()
            {
                Name = "Storefront.Checkout.Complete",
                Summary = "Complete checkout",
                Description = "Finalizes the checkout and places the order.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta SelectShippingMethod => new()
            {
                Name = "Storefront.Checkout.SelectShippingMethod",
                Summary = "Select shipping method",
                Description = "Assigns a shipping method to the order shipments.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta ListPaymentMethods => new()
            {
                Name = "Storefront.Checkout.ListPaymentMethods",
                Summary = "List payment methods",
                Description = "Retrieves a list of available payment methods for the checkout.",
                ResponseType = typeof(ApiResponse<List<CartModule.Checkout.ListPaymentMethods.Result>>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta CreatePayment => new()
            {
                Name = "Storefront.Checkout.CreatePayment",
                Summary = "Add payment",
                Description = "Adds a payment method to the checkout.",
                ResponseType = typeof(ApiResponse<Models.CartDetail>),
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
