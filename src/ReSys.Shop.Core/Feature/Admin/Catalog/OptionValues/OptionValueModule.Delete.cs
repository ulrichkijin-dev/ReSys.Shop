using ReSys.Shop.Core.Domain.Catalog.OptionTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.OptionValues;

public static partial class OptionValueModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                var idRequired = CommonInput.Errors.Required(prefix: nameof(OptionValue), nameof(OptionValue.Id));
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(idRequired.Code)
                    .WithMessage(idRequired.Description);
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext
        ) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
            {
                // Fetch: 
                var optionValue = await applicationDbContext.Set<OptionValue>()
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: cancellationToken);

                // Check: existence
                if (optionValue == null)
                    return OptionValue.Errors.NotFound(id: command.Id);

                // Check: deletable
                var deleteResult = optionValue.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                applicationDbContext.Set<OptionValue>().Remove(entity: optionValue);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }
    }
}