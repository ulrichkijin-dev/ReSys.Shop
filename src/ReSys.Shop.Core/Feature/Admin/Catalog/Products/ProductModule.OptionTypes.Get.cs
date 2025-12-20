using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Products.OptionTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class OptionTypes
    {
        public static class Get
        {
            public static class SelectList
            {
                public sealed class Request : QueryableParams
                {
                    [FromQuery] public Guid[]? ProductId { get; set; }
                }
                public sealed record Result : Models.ProductOptionTypeResult;
                public sealed record Query(Request Request) : IQuery<PaginationList<Result>>;
                public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper)
                    : IQueryHandler<Query, PaginationList<Result>>
                {
                    public async Task<ErrorOr<PaginationList<Result>>> Handle(Query command, CancellationToken ct)
                    {
                        var param = command.Request;
                        var pagedResult = await dbContext.Set<ProductOptionType>()
                            .Include(m => m.OptionType)
                            .Where(m =>
                                param.ProductId == null ||
                                param.ProductId.Length == 0 ||
                                param.ProductId.Contains(m.ProductId))
                            .AsNoTracking()
                            .ApplySearch(searchParams: command.Request)
                            .ApplyFilters(filterParams: command.Request)
                            .ApplySort(sortParams: command.Request)
                            .ProjectToType<Result>(config: mapper.Config)
                            .ToPagedListOrAllAsync(pagingParams: command.Request, cancellationToken: ct);

                        return pagedResult;
                    }
                }
            }
        }
    }
}