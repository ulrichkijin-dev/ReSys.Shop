using MapsterMapper;

using MediatR;

using ReSys.Shop.Core.Domain.Identity.UserAddresses;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Feature.Accounts.Addresses;

public static partial class AddressModule
{
    public static class GetById
    {
        public sealed record Result : AddressModule.Model.Detail;
        public record Query(Guid Id, string? UserId) : ICommand<Result>;

        public class Handler(IApplicationDbContext applicationDbContext, IMapper mapper) : IRequestHandler<Query, ErrorOr<Result>>
        {
            public async Task<ErrorOr<Result>> Handle(Query request, CancellationToken cancellationToken)
            {
                if (request.UserId == null)
                    return User.Errors.Unauthorized;

                var user = await applicationDbContext.Set<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == request.UserId,
                        cancellationToken: cancellationToken);

                if (user is null)
                    return User.Errors.NotFound(credential: request.UserId);

                var userAddress = await applicationDbContext.Set<UserAddress>()
                    .Include(navigationPropertyPath: a => a.State)
                    .Include(navigationPropertyPath: a => a.Country)
                    .FirstOrDefaultAsync(predicate: ua => ua.Id == request.Id,
                        cancellationToken: cancellationToken);

                if (userAddress is null)
                    return UserAddress.Errors.NotFound(id: request.Id);

                return mapper.Map<Result>(source: userAddress);
            }
        }
    }
}
