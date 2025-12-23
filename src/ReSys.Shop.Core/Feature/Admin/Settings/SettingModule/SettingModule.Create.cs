using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Settings;

namespace ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;

public static partial class SettingModule
{
    public static class Create
    {
        public sealed record Request : Models.Parameter;

        public sealed record Result : Models.ListItem;

        public sealed record Command(Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Request)
                    .SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public class CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);

                // For Setting, "Key" must be unique, similar to "Name" for OptionType
                var uniqueKeyCheck = await applicationDbContext.Set<Setting>()
                    .CheckKeyIsUniqueAsync<Setting, Guid>(
                        key: param.Key,
                        prefix: nameof(Setting),
                        cancellationToken: cancellationToken);

                if (uniqueKeyCheck.IsError)
                    return uniqueKeyCheck.Errors;

                var createResult = Setting.Create(
                    key: param.Key,
                    value: param.Value,
                    description: param.Description,
                    defaultValue: param.DefaultValue,
                    valueType: param.ValueType,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;
                var setting = createResult.Value;

                applicationDbContext.Set<Setting>().Add(entity: setting);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: setting);
            }
        }
    }
}
