using MapsterMapper;

using ReSys.Shop.Core.Domain.Promotions.Rules;


namespace  ReSys.Shop.Core.Feature.Admin.Promotions;

public static partial class PromotionModule
{
    public static partial class Rules
    {
        public static class Update
        {
            public sealed record Request : Models.RuleParameter;
            public sealed record Result : Models.RuleItem;
            public sealed record Command(Guid PromotionId, Guid RuleId, Request Request) : ICommand<Result>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.PromotionId)
                        .NotEmpty().WithMessage("Promotion ID is required.")
                        .WithErrorCode("Promotion.Id.Required");
                    RuleFor(x => x.RuleId)
                        .NotEmpty().WithMessage("Rule ID is required.")
                        .WithErrorCode("PromotionRule.Id.Required");
                    RuleFor(x => x.Request)
                        .SetValidator(new Models.RuleParameterValidator());
                }
            }

            public sealed class CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
                : ICommandHandler<Command, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
                {
                    var rule = await applicationDbContext.Set<PromotionRule>()
                        .Include(r => r.PromotionRuleTaxons)
                        .Include(r => r.PromotionRuleUsers)
                        .FirstOrDefaultAsync(r => r.Id == command.RuleId && r.PromotionId == command.PromotionId, ct);

                    if (rule == null)
                        return PromotionRule.Errors.NotFound(command.RuleId);

                    var updateResult = rule.Update(value: command.Request.Value);
                    if (updateResult.IsError) return updateResult.Errors;

                    await applicationDbContext.SaveChangesAsync(ct);

                    return mapper.Map<Result>(rule);
                }
            }
        }
    }
}