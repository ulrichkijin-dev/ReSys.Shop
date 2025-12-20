using ReSys.Shop.Core.Domain.Catalog.Products;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
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
                var product = await applicationDbContext.Set<Product>()
                    .Include(navigationPropertyPath: p => p.Variants)
                    .ThenInclude(navigationPropertyPath: v => v.LineItems)
                    .ThenInclude(navigationPropertyPath: li => li.Order)
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: ct);

                if (product == null)
                    return Product.Errors.NotFound(id: command.Id);

                var deleteResult = product.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Deleted;
            }
        }
    }
}