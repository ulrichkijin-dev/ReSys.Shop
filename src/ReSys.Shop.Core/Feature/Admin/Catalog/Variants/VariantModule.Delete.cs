using ReSys.Shop.Core.Domain.Catalog.Products.Variants;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Variants;

public static partial class VariantModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(IApplicationDbContext applicationDbContext)
            : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
            {
                var variant = await applicationDbContext.Set<Variant>()
                    .Include(navigationPropertyPath: v => v.LineItems)
                    .ThenInclude(navigationPropertyPath: li => li.Order)
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: ct);

                if (variant == null)
                    return Variant.Errors.NotFound(id: command.Id);

                var deleteResult = variant.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Deleted;
            }
        }
    }
}