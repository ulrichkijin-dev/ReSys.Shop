using ReSys.Shop.Core.Domain.Catalog.Products;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.Products;

public static partial class ProductModule
{
    public static partial class Statuses
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
                    var product = await applicationDbContext.Set<Product>()
                        .FindAsync(keyValues: [command.Id], cancellationToken: ct);

                    if (product == null)
                        return Product.Errors.NotFound(id: command.Id);

                    await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                    var discontinueResult = product.Discontinue();
                    if (discontinueResult.IsError) return discontinueResult.Errors;

                    await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                    await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                    return Result.Success;
                }
            }
        }
    }
}