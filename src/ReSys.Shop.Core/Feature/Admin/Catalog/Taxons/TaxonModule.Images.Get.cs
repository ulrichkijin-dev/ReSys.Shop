using Mapster;

using MapsterMapper;

using Microsoft.Extensions.Logging;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Images;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Taxons;

public static partial class TaxonModule
{
    public static partial class Images
    {
        public static class Get
        {
            public sealed class Request : QueryableParams;
            public sealed class Result : Models.UploadImageParameter;
            public sealed record Query(Guid TaxonId, Request Request) : IQuery<PaginationList<Result>>;

            public sealed class QueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper, ILogger<QueryHandler> logger)
                : IQueryHandler<Query, PaginationList<Result>>
            {
                public async Task<ErrorOr<PaginationList<Result>>> Handle(Query request, CancellationToken ct)
                {
                    try
                    {
                        PaginationList<Result> rules = await applicationDbContext.Set<TaxonImage>()
                            .Where(predicate: r => r.TaxonId == request.TaxonId)
                            .OrderBy(keySelector: r => r.CreatedAt)
                            .AsQueryable()
                            .AsNoTracking()
                            .ApplySearch(searchParams: request.Request)
                            .ApplyFilters(filterParams: request.Request)
                            .ApplySort(sortParams: request.Request)
                            .ProjectToType<Result>(config: mapper.Config)
                            .ToPagedListOrAllAsync(pagingParams: request.Request, cancellationToken: ct);

                        return rules;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(exception: ex, message: "Error retrieving rules for taxon {TaxonId}", args: request.TaxonId);
                        return Error.Failure(code: "TaxonImages.GetFailed", description: "Failed to retrieve taxon rules");
                    }
                }
            }
        }
    }
}