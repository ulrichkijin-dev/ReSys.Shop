using Mapster;
using MapsterMapper;
using ReSys.Shop.Core.Common.Models.Filter;
using ReSys.Shop.Core.Common.Models.Search;
using ReSys.Shop.Core.Common.Models.Sort;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products;

namespace ReSys.Shop.Core.Feature.Storefront.Products;

public static partial class ProductModule
{
    public static class Get
    {
        public static class PagedList
        {
            public sealed record Query(Guid? TaxonId, QueryableParams Params) : IQuery<PaginationList<Models.ProductItem>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.ProductItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.ProductItem>>> Handle(Query request, CancellationToken ct)
                {
                    var query = dbContext.Set<Product>()
                        .Include(p => p.Variants).ThenInclude(v => v.Prices)
                        .Include(p => p.Images)
                        .Where(p => p.Status == Product.ProductStatus.Active && !p.IsDeleted)
                        .AsNoTracking();

                    if (request.TaxonId.HasValue)
                    {
                        query = query.Where(p => p.Classifications.Any(c => c.TaxonId == request.TaxonId.Value));
                    }

                    query = query.ApplySearch(request.Params)
                                 .ApplyFilters(request.Params)
                                 .ApplySort(request.Params);

                    if (string.IsNullOrWhiteSpace(request.Params.SortBy))
                    {
                        query = ((IQueryable<Product>)query).OrderByDescending(p => p.CreatedAt);
                    }

                    return await query
                        .ProjectToType<Models.ProductItem>(mapper.Config)
                        .ToPagedListAsync(request.Params, ct);
                }
            }
        }

        public static class BySlug
        {
            public sealed record Query(string Slug) : IQuery<Models.ProductDetail>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                : IQueryHandler<Query, Models.ProductDetail>
            {
                public async Task<ErrorOr<Models.ProductDetail>> Handle(Query request, CancellationToken ct)
                {
                    var product = await dbContext.Set<Product>()
                        .Include(p => p.Variants).ThenInclude(v => v.Prices)
                        .Include(p => p.Variants).ThenInclude(v => v.StockItems)
                        .Include(p => p.Images)
                        .Include(p => p.ProductPropertyTypes).ThenInclude(pp => pp.PropertyType)
                        .Where(p => p.Slug == request.Slug && p.Status == Product.ProductStatus.Active && !p.IsDeleted)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ct);

                    if (product == null) return Product.Errors.NotFound(Guid.Empty);

                    return mapper.Map<Models.ProductDetail>(product);
                }
            }
        }
    }
}
