using Mapster;

using MapsterMapper;

using  ReSys.Shop.Core.Common.Models.Filter;
using  ReSys.Shop.Core.Common.Models.Search;
using  ReSys.Shop.Core.Common.Models.Sort;
using  ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using  ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Domain.Promotions.Rules;


namespace  ReSys.Shop.Core.Feature.Admin.Promotions;

public static partial class PromotionModule
{
    public static partial class Rules
    {
        public static class Get
        {
            public sealed class Request : QueryableParams;
            public sealed record Query(Guid PromotionId, Request Request) : IQuery<PaginationList<Models.RuleItem>>;

            public sealed class QueryHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
                : IQueryHandler<Query, PaginationList<Models.RuleItem>>
            {
                public async Task<ErrorOr<PaginationList<Models.RuleItem>>> Handle(Query request, CancellationToken ct)
                {
                    var rules = await applicationDbContext.Set<PromotionRule>()
                        .Where(r => r.PromotionId == request.PromotionId)
                        .Include(r => r.PromotionRuleTaxons)
                        .Include(r => r.PromotionRuleUsers)
                        .OrderBy(r => r.CreatedAt)
                        .AsNoTracking()
                        .ApplySearch(request.Request)
                        .ApplyFilters(request.Request)
                        .ApplySort(request.Request)
                        .ProjectToType<Models.RuleItem>(mapper.Config)
                        .ToPagedListOrAllAsync(request.Request, ct);

                    return rules;
                }
            }
        }
    }
}