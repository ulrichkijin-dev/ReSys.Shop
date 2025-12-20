using ReSys.Shop.Core.Domain.Settings.PaymentMethods;


namespace  ReSys.Shop.Core.Feature.Admin.Settings.PaymentMethods;

public static partial class PaymentMethodModule
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
                var paymentMethod = await applicationDbContext.Set<PaymentMethod>()
                    .Include(pm => pm.Payments)
                    .FirstOrDefaultAsync(predicate: pm => pm.Id == command.Id, cancellationToken: ct);

                if (paymentMethod == null)
                    return PaymentMethod.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var deleteResult = paymentMethod.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Deleted;
            }
        }
    }
}