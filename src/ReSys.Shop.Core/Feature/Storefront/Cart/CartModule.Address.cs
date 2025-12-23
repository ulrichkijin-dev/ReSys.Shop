using MapsterMapper;


using ReSys.Shop.Core.Domain.Identity.UserAddresses;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Addresses
    {
        public record AddressRequest(
            string FirstName,
            string LastName,
            string Address1,
            string? Address2,
            string City,
            string ZipCode,
            string Phone,
            Guid CountryId,
            Guid? StateId,
            string? Company
        );

        public static class SetShippingAddress
        {
            public sealed record Command(AddressRequest Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var userId = userContext.UserId ?? userContext.AdhocCustomerId ?? "guest";

                    var addressResult = UserAddress.Create(
                        firstName: command.Request.FirstName,
                        lastName: command.Request.LastName,
                        userId: userId,
                        countryId: command.Request.CountryId,
                        address1: command.Request.Address1,
                        city: command.Request.City,
                        zipcode: command.Request.ZipCode,
                        stateId: command.Request.StateId,
                        address2: command.Request.Address2,
                        phone: command.Request.Phone,
                        company: command.Request.Company,
                        type: AddressType.Shipping
                    );

                    if (addressResult.IsError) return addressResult.Errors;

                    // Note: In a real scenario with Guests, we might need to handle UserAddress persistence differently
                    // if the FK constraint to Users table is strict. 
                    // Assuming for this fix that we can create the address object and attach it.
                    // If UserAddress is an Aggregate Root, it should be added to its DbSet.
                    // However, SetShippingAddress on Order takes a UserAddress.
                    
                    // Since UserAddress is an Aggregate, we should add it to the context?
                    // Or relies on cascade? Order.SetShippingAddress sets the navigation property.
                    // If we add it to the Order.ShipAddress, EF Core should handle it if mapped correctly.
                    
                    var address = addressResult.Value;
                    
                    // Fix for Guest User ID constraint if strictly validated by DB:
                    // If userId is "guest" or Adhoc ID, it might fail FK if 'Users' table doesn't have it.
                    // Ideally, Order should use an Owned Entity for address to avoid this coupling.
                    // PROPOSAL: We just set the address on the order.
                    
                    var result = cart.SetShippingAddress(address);
                    if (result.IsError) return result.Errors;
                    
                    // If Billing is not set, set it to the same for convenience (or handle separately)
                    if (cart.BillAddress == null)
                    {
                         var billAddressResult = UserAddress.Create(
                            firstName: command.Request.FirstName,
                            lastName: command.Request.LastName,
                            userId: userId,
                            countryId: command.Request.CountryId,
                            address1: command.Request.Address1,
                            city: command.Request.City,
                            zipcode: command.Request.ZipCode,
                            stateId: command.Request.StateId,
                            address2: command.Request.Address2,
                            phone: command.Request.Phone,
                            company: command.Request.Company,
                            type: AddressType.Billing
                        );
                        if (!billAddressResult.IsError)
                        {
                            cart.SetBillingAddress(billAddressResult.Value);
                        }
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }
    }
}
