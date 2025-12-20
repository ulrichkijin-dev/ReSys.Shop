using ReSys.Shop.Core.Domain.Catalog.Products.Variants;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Variants;

public static partial class VariantModule
{
    public static class Discontinue
    {
        public sealed record Command(Guid Id) : ICommand<Success>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(IApplicationDbContext applicationDbContext)
            : ICommandHandler<Command, Success>
        {
            public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
            {
                var variant = await applicationDbContext.Set<Variant>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);

                if (variant == null)
                    return Variant.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var discontinueResult = variant.Discontinue();
                if (discontinueResult.IsError) return discontinueResult.Errors;

                applicationDbContext.Set<Variant>().Update(variant);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Success;
            }
        }
    }
}