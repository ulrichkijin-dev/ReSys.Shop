using ReSys.Shop.Core.Domain.Settings.ShippingMethods;


namespace  ReSys.Shop.Core.Feature.Admin.Settings.ShippingMethods;

public static partial class ShippingMethodModule
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
                var shippingMethod = await applicationDbContext.Set<ShippingMethod>()
                    .FirstOrDefaultAsync(predicate: sm => sm.Id == command.Id, cancellationToken: ct);

                if (shippingMethod == null)
                    return ShippingMethod.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var deleteResult = shippingMethod.Delete(); // Assuming this performs a soft delete on the domain model
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Deleted;
            }
        }
    }
}
