using ReSys.Shop.Core.Domain.Promotions.Promotions;


namespace  ReSys.Shop.Core.Feature.Admin.Promotions;

public static partial class PromotionModule
{
    public static class Activate
    {
        public sealed record Command(Guid Id) : ICommand<Success>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty().WithMessage("Promotion ID is required.")
                    .WithErrorCode("Promotion.Id.Required");
            }
        }

        public sealed class CommandHandler(IApplicationDbContext applicationDbContext) : ICommandHandler<Command, Success>
        {
            public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
            {
                var promotion = await applicationDbContext.Set<Promotion>()
                    .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

                if (promotion == null)
                    return Promotion.Errors.NotFound(command.Id);

                var activateResult = promotion.Activate();
                if (activateResult.IsError) return activateResult.Errors;

                await applicationDbContext.SaveChangesAsync(ct);
                return Result.Success;
            }
        }
    }
}