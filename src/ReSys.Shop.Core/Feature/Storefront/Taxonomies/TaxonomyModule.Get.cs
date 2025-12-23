using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;

namespace ReSys.Shop.Core.Feature.Storefront.Taxonomies;

public static partial class TaxonomyModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed record Query(QueryableParams Params) : IQuery<PaginationList<Models.TaxonomyItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.TaxonomyItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.TaxonomyItem>>> Handle(Query request, CancellationToken ct)
                {
                    var query = dbContext.Set<Taxonomy>()
                        .Include(t => t.Taxons)
                        .AsNoTracking();

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<Taxonomy>)query).OrderBy(t => t.Position);
                    }

                    return await query
                        .ProjectToType<Models.TaxonomyItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class ById
        {
            public sealed record Query(Guid Id) : IQuery<Models.TaxonomyItem>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.TaxonomyItem>
            {
                public async Task<ErrorOr<Models.TaxonomyItem>> Handle(Query request, CancellationToken ct)
                {
                    var taxonomy = await dbContext.Set<Taxonomy>()
                        .Include(t => t.Taxons)
                        .Where(t => t.Id == request.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (taxonomy == null) return Taxonomy.Errors.NotFound(request.Id);

                    return mapper.Map<Models.TaxonomyItem>(taxonomy);
                }
            }
        }
    }
}
