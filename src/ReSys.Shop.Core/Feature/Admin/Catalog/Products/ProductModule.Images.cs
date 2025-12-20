using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class Images
    {
        public static class GetList
        {
            public sealed class Request : QueryableParams
            {
                [FromQuery] public Guid[]? ProductId { get; set; }
                [FromQuery] public Guid[]? VariantId { get; set; }
            }

            public sealed class Result : Models.ImageResult;

            public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(
                IApplicationDbContext applicationDbContext,
                IMapper mapper)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(
                    Query request,
                    CancellationToken ct)
                {
                    var param = request.Request;

                    var query = applicationDbContext.Set<ProductImage>()
                        .AsNoTracking()
                        .AsQueryable()
                        .Where(i =>
                            (param.ProductId == null || param.ProductId.Length == 0 || i.ProductId == null || param.ProductId.ToList().Contains(i.ProductId.Value)) &&
                            (param.VariantId == null || param.VariantId.Length == 0 || i.VariantId == null || param.VariantId.ToList().Contains(i.VariantId.Value)));

                    query = query
                        .ApplySearch(param)
                        .ApplyFilters(param)
                        .ApplySort(param);

                    var result = await query
                        .OrderBy(i => i.Position)
                        .ProjectToType<Result>(mapper.Config)
                        .ToPagedListOrAllAsync(
                            pagingParams: param,
                            cancellationToken: ct);

                    return result;
                }
            }
        }

    }
}