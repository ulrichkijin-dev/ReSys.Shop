using ReSys.Shop.Core.Domain.Settings;

// Changed from Catalog.OptionTypes

// For AbstractValidator

namespace ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;

public static partial class SettingModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(CommonInput.Errors.Required(prefix: nameof(Setting), nameof(Setting.Id)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix: nameof(Setting), nameof(Setting.Id)).Description);
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext
        ) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
            {
                // Fetch:
                var setting = await applicationDbContext.Set<Setting>()
                    .FirstOrDefaultAsync(predicate: m => m.Id == command.Id, cancellationToken: cancellationToken);

                // Check: existence
                if (setting == null)
                    return Setting.Errors.NotFound; // Changed from OptionType.Errors.NotFound(id: command.Id);

                // Setting entity does not have a Delete() method like OptionType, so direct removal.
                // If there were business rules for deletion, they would be here.

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                applicationDbContext.Set<Setting>().Remove(entity: setting);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }
    }
}
