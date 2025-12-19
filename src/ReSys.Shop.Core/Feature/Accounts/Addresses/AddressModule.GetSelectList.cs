using Mapster;

using MapsterMapper;

using MediatR;

using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Feature.Accounts.Addresses;

public static partial class AddressModule
{
    public static class GetSelectList
    {
        public sealed class Param : QueryableParams;
        public sealed record Result : Model.SelectItem;
        public record Query(string? UserId, Param Param) : IRequest<ErrorOr<PaginationList<Result>>>;

        public class Handler(IApplicationDbContext applicationDbContext,IMapper mapper) : IRequestHandler<Query, ErrorOr<PaginationList<Result>>>
        {
            public async Task<ErrorOr<PaginationList<Result>>> Handle(Query request, CancellationToken cancellationToken)
            {
                if (request.UserId == null)
                    return User.Errors.Unauthorized;

                var user = await applicationDbContext.Set<User>()
                    .Include(navigationPropertyPath: u => u.UserAddresses)
                    .FirstOrDefaultAsync(predicate: u => u.Id == request.UserId,
                        cancellationToken: cancellationToken);

                if (user is null)
                    return User.Errors.NotFound(credential: request.UserId);

                var items = await user.UserAddresses
                    .AsQueryable()
                    .Include(navigationPropertyPath: a => a.State)
                    .ApplySearch(searchParams: request.Param)
                    .ApplyFilters(filterParams: request.Param)
                    .ApplySort(sortParams: request.Param)
                    .ProjectToType<Result>(config: mapper.Config)
                    .ToPagedListAsync(pagingParams: request.Param,
                        cancellationToken: cancellationToken);

                return items;
            }
        }
    }
}
