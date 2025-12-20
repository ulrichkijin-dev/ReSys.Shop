using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.OptionTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.OptionValues;

public static partial class OptionValueModule
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

        public class CommandHandler(IApplicationDbContext applicationDbContext, IMapper mapper) : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);

                // Check if OptionType exists
                OptionType? optionType = await applicationDbContext.Set<OptionType>()
                    .FindAsync(keyValues: [param.OptionTypeId], cancellationToken: cancellationToken);
                if (optionType == null)
                {
                    return OptionType.Errors.NotFound(id: param.OptionTypeId);
                }
                var uniqueNameCheck = await applicationDbContext.Set<OptionValue>()
                    .CheckNameIsUniqueAsync<OptionValue, Guid>(
                        name: param.Name,
                        prefix: nameof(OptionValue),
                        cancellationToken: cancellationToken);

                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                var createResult = OptionValue.Create(
                    optionTypeId: param.OptionTypeId,
                    name: param.Name,
                    presentation: param.Presentation,
                    position: param.Position,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;
                var optionValue = createResult.Value;

                applicationDbContext.Set<OptionValue>().Add(entity: optionValue);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: optionValue);
            }
        }
    }
}