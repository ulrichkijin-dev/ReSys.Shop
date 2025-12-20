using ReSys.Shop.Core.Domain.Promotions.Promotions;


namespace  ReSys.Shop.Core.Feature.Admin.Promotions;

public static partial class PromotionModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty().WithMessage("Promotion ID is required.")
                    .WithErrorCode("Promotion.Id.Required");
            }
        }
        public sealed class CommandHandler(IApplicationDbContext applicationDbContext) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
            {
                var promotion = await applicationDbContext.Set<Promotion>()
                    .Include(p => p.PromotionRules)
                    .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

                if (promotion == null)
                    return Promotion.Errors.NotFound(command.Id);

                // Check if promotion has been used
                if (promotion.UsageCount > 0)
                    return Error.Validation("Promotion.CannotDeleteUsed",
                        "Cannot delete a promotion that has been used. Consider deactivating it instead.");

                applicationDbContext.Set<Promotion>().Remove(promotion);
                await applicationDbContext.SaveChangesAsync(ct);

                return Result.Deleted;
            }
        }
    }
}