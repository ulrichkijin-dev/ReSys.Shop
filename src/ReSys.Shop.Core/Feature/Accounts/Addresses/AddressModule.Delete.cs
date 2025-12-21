using ReSys.Shop.Core.Domain.Identity.UserAddresses;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Feature.Accounts.Addresses;

public static partial class AddressModule
{
    public static class Delete
    {
        public record Command(Guid Id, string? UserId) : IRequest<ErrorOr<Deleted>>;
        public class Handler(IApplicationDbContext applicationDbContext) : IRequestHandler<Command, ErrorOr<Deleted>>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.UserId == null)
                    return User.Errors.Unauthorized;

                var user = await applicationDbContext.Set<User>()
                    .Include(navigationPropertyPath: u => u.UserAddresses)
                    .FirstOrDefaultAsync(predicate: u => u.Id == request.UserId,
                        cancellationToken: cancellationToken);

                if (user is null)
                    return User.Errors.NotFound(credential: request.UserId);

                var userAddress = user.UserAddresses.FirstOrDefault(predicate: ua => ua.Id == request.Id);
                if (userAddress is null)
                {
                    return UserAddress.Errors.NotFound(id: request.Id);
                }

                var deleteResult = userAddress.Delete();
                if (deleteResult.IsError)
                {
                    return deleteResult.FirstError;
                }

                user.UserAddresses.Remove(item: userAddress);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }
    }
}
