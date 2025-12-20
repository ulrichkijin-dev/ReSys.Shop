using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.Classifications;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class Classifications
    {
        public static class Get
        {
            public static class SelectList
            {
                public sealed class Request : QueryableParams
                {
                    [FromQuery] public Guid[]? ProductId { get; set; }
                    [FromQuery] public Guid[]? TaxonId { get; set; }
                }

                public sealed record Result : Models.ProductClassificationResult;

                public sealed record Query(Request Request)
                    : IQuery<PaginationList<Result>>;

                public sealed class QueryHandler(
                    IApplicationDbContext dbContext,
                    IMapper mapper)
                    : IQueryHandler<Query, PaginationList<Result>>
                {
                    public async Task<ErrorOr<PaginationList<Result>>> Handle(
                        Query query,
                        CancellationToken ct)
                    {
                        var p = query.Request;

                        var result = await dbContext.Set<Classification>()
                            .Include(c => c.Taxon)
                            .Where(c =>
                                (p.ProductId == null || p.ProductId.Length == 0 || p.ProductId.Contains(c.ProductId)) &&
                                (p.TaxonId == null || p.TaxonId.Length == 0 || p.TaxonId.Contains(c.TaxonId)))
                            .AsNoTracking()
                            .ApplySearch(p)
                            .ApplyFilters(p)
                            .ApplySort(p)
                            .ProjectToType<Result>(mapper.Config)
                            .ToPagedListOrAllAsync(p, ct);

                        return result;
                    }
                }
            }
        }
    }
}