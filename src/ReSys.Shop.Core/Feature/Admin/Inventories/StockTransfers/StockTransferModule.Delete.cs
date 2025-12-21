using ReSys.Shop.Core.Domain.Inventories.StockTransfers;


namespace  ReSys.Shop.Core.Feature.Admin.Inventories.StockTransfers;

public static partial class StockTransferModule
{
    // Delete
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
                var transfer = await applicationDbContext.Set<StockTransfer>()
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: ct);

                if (transfer == null)
                    return StockTransfer.Errors.NotFound(id: command.Id);

                var deleteResult = transfer.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);
                applicationDbContext.Set<StockTransfer>().Remove(entity: transfer);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return Result.Deleted;
            }
        }
    }
}