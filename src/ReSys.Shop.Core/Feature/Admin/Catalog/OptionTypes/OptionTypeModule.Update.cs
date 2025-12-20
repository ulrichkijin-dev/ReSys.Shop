using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.OptionTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.OptionTypes;

public static partial class OptionTypeModule
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
                    .WithErrorCode(CommonInput.Errors.Required(prefix: nameof(OptionType), nameof(OptionType.Id)).Code)
                    .WithMessage(CommonInput.Errors.Required(prefix: nameof(OptionType), nameof(OptionType.Id)).Description);

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
                    .FindAsync(keyValues: [command.Id], cancellationToken: cancellationToken);
                if (optionType == null)
                {
                    return OptionType.Errors.NotFound(id: command.Id);
                }

                if (optionType.Name != request.Name)
                {
                    var uniqueNameCheck = await applicationDbContext.Set<OptionType>()
                        .Where(predicate: m => m.Id != optionType.Id)
                        .CheckNameIsUniqueAsync<OptionType, Guid>(
                            name: request.Name,
                            prefix: nameof(OptionType),
                            cancellationToken: cancellationToken, 
                            exclusions: [optionType.Id]);
                    if (uniqueNameCheck.IsError)
                        return uniqueNameCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);
                var updateResult = optionType.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    filterable: request.Filterable,
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