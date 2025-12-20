using MapsterMapper;

using ReSys.Shop.Core.Domain.Settings.ShippingMethods;


namespace  ReSys.Shop.Core.Feature.Admin.Settings.ShippingMethods;

public static partial class ShippingMethodModule
{
    public static class Deactivate
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
                var shippingMethod = await applicationDbContext.Set<ShippingMethod>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);
                if (shippingMethod == null)
                    return ShippingMethod.Errors.NotFound(id: command.Id);

                if (!shippingMethod.Active) // Already inactive
                    return mapper.Map<Result>(source: shippingMethod);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var updateResult = shippingMethod.Update(active: false);
                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: shippingMethod);
            }
        }
    }
}
