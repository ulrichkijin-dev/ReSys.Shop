using Mapster;

using MapsterMapper;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxons;

public static partial class TaxonModule
{
    public static partial class Get
    {
        public static class PagedList
        {
            public sealed class Request : QueryableParams;
            public sealed record Result : Models.ListItem;
            public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query command, CancellationToken ct)
                {
                    var pagedResult = await dbContext.Set<Taxon>()
                        .Include(navigationPropertyPath: t => t.Taxonomy)
                        .Include(navigationPropertyPath: t => t.Parent)
                        .Include(navigationPropertyPath: t => t.Children)
                        .Include(navigationPropertyPath: t => t.TaxonImages)
                        .AsNoTracking()
                        .ApplySearch(searchParams: command.Request)
                        .ApplyFilters(filterParams: command.Request)
                        .ApplySort(sortParams: command.Request)
                        .ProjectToType<Result>(config: mapper.Config)
                        .ToPagedListAsync(pagingParams: command.Request, cancellationToken: ct);

                    return pagedResult;
                }
            }
        }
    }
}