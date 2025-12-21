using ReSys.Shop.Core.Domain.Inventories.Locations;


namespace  ReSys.Shop.Core.Feature.Admin.Inventories.StockLocations;

public static partial class StockLocationModule
{
    public static class Restore
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
                var location = await applicationDbContext.Set<StockLocation>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: ct);

                if (location == null)
                    return StockLocation.Errors.NotFound(id: command.Id);

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var restoreResult = location.Restore();
                if (restoreResult.IsError) return restoreResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Success;
            }
        }
    }
}
