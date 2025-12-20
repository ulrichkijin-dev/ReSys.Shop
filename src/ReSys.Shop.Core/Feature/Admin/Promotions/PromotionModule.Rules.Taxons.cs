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
        public static partial class Taxons
        {
            public static class Get
            {
                public sealed class Request : QueryableParams;
                public sealed record Query(Guid PromotionId, Guid RuleId, Request Request) : IQuery<PaginationList<Models.PromotionTaxonRuleItem>>;

                public sealed class QueryHandler(IApplicationDbContext dbContext)
                    : IQueryHandler<Query, PaginationList<Models.PromotionTaxonRuleItem>>
                {
                    public async Task<ErrorOr<PaginationList<Models.PromotionTaxonRuleItem>>> Handle(Query request, CancellationToken ct)
                    {
                        // First, check if the rule exists and belongs to the promotion
                        var ruleExists = await dbContext.Set<PromotionRule>()
                            .AnyAsync(r => r.Id == request.RuleId && r.PromotionId == request.PromotionId, ct);

                        if (!ruleExists)
                            return PromotionRule.Errors.NotFound(request.RuleId);

                        var query = dbContext.Set<PromotionRuleTaxon>()
                            .Where(prt => prt.PromotionRuleId == request.RuleId)
                            .Include(prt => prt.Taxon)
                            .AsNoTracking();

                        var pagedResult = await query
                            .ApplySearch(request.Request)
                            .ApplyFilters(request.Request)
                            .ApplySort(request.Request)
                            .Select(prt => new Models.PromotionTaxonRuleItem
                            {
                                Id = prt.Taxon.Id,
                                TaxonId = prt.Taxon.Id,
                                TaxonName = prt.Taxon.Name,
                                CreatedAt = prt.CreatedAt
                            })
                            .ToPagedListOrAllAsync(request.Request, ct);

                        return pagedResult;
                    }
                }
            }
        }
    }
}