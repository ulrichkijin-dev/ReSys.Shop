using MapsterMapper;

using ReSys.Shop.Core.Domain.Settings.PaymentMethods;


namespace  ReSys.Shop.Core.Feature.Admin.Settings.PaymentMethods;

public static partial class PaymentMethodModule
{
    public static class Restore
    {
        public sealed record Command(Guid Id) : ICommand<Result>;
        public sealed record Result : Models.Detail;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var paymentMethod = await applicationDbContext.Set<PaymentMethod>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);
                if (paymentMethod == null)
                    return PaymentMethod.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var restoreResult = paymentMethod.Restore();
                if (restoreResult.IsError) return restoreResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: restoreResult.Value);
            }
        }
    }
}