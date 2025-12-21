using Mapster;

using MapsterMapper;

using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Identity.UserAddresses;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Feature.Accounts.Addresses;

public static partial class AddressModule
{
    public static class GetPagedList
    {
        public sealed class Param : QueryableParams;

        public sealed record Result : Model.ListItem;
        public record Query(string? UserId, Param Param) : IRequest<ErrorOr<PaginationList<Result>>>;

        public class Handler(IApplicationDbContext applicationDbContext, IMapper mapper) : IRequestHandler<Query, ErrorOr<PaginationList<Result>>>
        {
            public async Task<ErrorOr<PaginationList<Result>>> Handle(Query request, CancellationToken cancellationToken)
            {
                if (request.UserId == null)
                    return User.Errors.Unauthorized;

                var user = await applicationDbContext.Set<User>()
                    .FirstOrDefaultAsync(predicate: u => u.Id == request.UserId,
                        cancellationToken: cancellationToken);
                if (user is null)
                    return User.Errors.NotFound(credential: request.UserId);

                var query = applicationDbContext.Set<UserAddress>()
                    .Include(navigationPropertyPath: a => a.State);

                PaginationList<Result> pagedResult = await query
                    .ApplySearch(searchParams: request.Param)
                    .ApplyFilters(filterParams: request.Param)
                    .ApplySort(sortParams: request.Param)
                    .ProjectToType<Result>(config: mapper.Config)
                    .ToPagedListAsync(pagingParams: request.Param,
                        cancellationToken: cancellationToken);

                return pagedResult;
            }
        }
    }
}
