using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Settings;

// Changed from Catalog.OptionTypes

// For AbstractValidator

namespace ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;

public static partial class SettingModule
{
    public static class Update
    {
        public record Request : Models.Parameter;
        public record Result : Models.ListItem;
        public sealed record Command(Guid Id, Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(CommonInput.Errors.Required(prefix: nameof(Setting), nameof(Setting.Id)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix: nameof(Setting), nameof(Setting.Id)).Description);

                RuleFor(expression: x => x.Request)
                    .SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper
        ) : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
            {
                var request = command.Request;

                Setting? setting = await applicationDbContext.Set<Setting>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: cancellationToken);
                if (setting == null)
                {
                    return Setting.Errors.NotFound; // Changed from OptionType.Errors.NotFound(id: command.Id);
                }

                if (setting.Key != request.Key) // Check if Key has changed
                {
                    var uniqueKeyCheck = await applicationDbContext.Set<Setting>()
                        .Where(predicate: m => m.Id != setting.Id)
                        .CheckKeyIsUniqueAsync<Setting, Guid>(
                            key: request.Key,
                            prefix: nameof(Setting),
                            cancellationToken: cancellationToken,
                            exclusions: [setting.Id]);
                    if (uniqueKeyCheck.IsError)
                        return uniqueKeyCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                
                var updateResult = setting.Update(
                    newValue: request.Value,
                    description: request.Description,
                    defaultValue: request.DefaultValue,
                    valueType: request.ValueType,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: request.PrivateMetadata); 

                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: setting); // Map the updated setting
            }
        }
    }
}
