using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Shop.Core.Feature.Storefront.Taxons;

public static partial class TaxonModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed record Query(QueryableParams Params) : IQuery<PaginationList<Models.TaxonItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.TaxonItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.TaxonItem>>> Handle(Query request, CancellationToken ct)
                {
                    var query = dbContext.Set<Taxon>()
                        .Where(t => !t.HideFromNav)
                        .AsNoTracking();

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<Taxon>)query).OrderBy(t => t.TaxonomyId).ThenBy(t => t.Lft);
                    }

                    return await query
                        .ProjectToType<Models.TaxonItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class ById
        {
            public sealed record Query(Guid Id) : IQuery<Models.TaxonItem>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.TaxonItem>
            {
                public async Task<ErrorOr<Models.TaxonItem>> Handle(Query request, CancellationToken ct)
                {
                    var taxon = await dbContext.Set<Taxon>()
                        .Include(t => t.Children)
                        .Where(t => t.Id == request.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (taxon == null) return Taxon.Errors.NotFound(request.Id);

                    return mapper.Map<Models.TaxonItem>(taxon);
                }
            }
        }
    }
}
