using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.OptionTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.OptionValues;

public static partial class OptionValueModule
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
                var idRequired = CommonInput.Errors.Required(prefix: nameof(OptionValue), nameof(OptionValue.Id));
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithErrorCode(idRequired.Code)
                    .WithMessage(idRequired.Description);

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
                OptionType? optionType = await applicationDbContext.Set<OptionType>()
                    .FindAsync(keyValues: [request.OptionTypeId], cancellationToken: cancellationToken);

                if (optionType == null)
                    return OptionType.Errors.NotFound(id: request.OptionTypeId);

                var optionValue = await applicationDbContext.Set<OptionValue>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: cancellationToken);
                if (optionValue == null)
                {
                    return OptionValue.Errors.NotFound(id: command.Id);
                }

                if (optionValue.Name != request.Name)
                {
                    var uniqueNameCheck = await applicationDbContext.Set<OptionValue>()
                        .Where(predicate: m => m.Id != optionValue.Id)
                        .CheckNameIsUniqueAsync<OptionValue, Guid>(
                            name: request.Name,
                            prefix: nameof(OptionValue),
                            cancellationToken: cancellationToken, 
                            exclusions: optionValue.Id);
                    if (uniqueNameCheck.IsError)
                        return uniqueNameCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                var updateResult = optionValue.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    position: request.Position,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: request.PrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: updateResult.Value);

            }
        }
    }
}