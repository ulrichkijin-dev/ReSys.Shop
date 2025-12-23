using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Location.States;

namespace ReSys.Shop.Core.Feature.Storefront.Countries;

public static partial class CountryModule
{
    public static class States
    {
        public static class PagedList
        {
            public sealed record Query(Guid? CountryId, QueryableParams Params) : IQuery<PaginationList<Models.StateItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.StateItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.StateItem>>> Handle(Query request, CancellationToken ct)
                {
                    var query = dbContext.Set<State>()
                        .AsNoTracking();

                    if (request.CountryId.HasValue)
                    {
                        query = query.Where(s => s.CountryId == request.CountryId.Value);
                    }

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<State>)query).OrderBy(s => s.Name);
                    }

                    return await query
                        .ProjectToType<Models.StateItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class ById
        {
            public sealed record Query(Guid Id) : IQuery<Models.StateItem>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.StateItem>
            {
                public async Task<ErrorOr<Models.StateItem>> Handle(Query request, CancellationToken ct)
                {
                    var state = await dbContext.Set<State>()
                        .Where(s => s.Id == request.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (state == null) return State.Errors.NotFound(request.Id);

                    return mapper.Map<Models.StateItem>(state);
                }
            }
        }
    }
}
