using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.PropertyTypes;


namespace  ReSys.Shop.Core.Feature.Admin.Catalog.PropertyTypes;

public static partial class PropertyTypeModule
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

                var uniqueNameCheck = await applicationDbContext.Set<PropertyType>()
                    .CheckNameIsUniqueAsync<PropertyType, Guid>(
                        name: param.Name,
                        prefix: nameof(PropertyType),
                        cancellationToken: cancellationToken);

                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                var createResult = PropertyType.Create(
                    name: param.Name,
                    presentation: param.Presentation,
                    kind: param.Kind,
                    filterable: param.Filterable,
                    displayOn: param.DisplayOn,
                    position: param.Position,
                    filterParam: param.FilterParam,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;
                var property = createResult.Value;

                applicationDbContext.Set<PropertyType>().Add(entity: property);
                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return mapper.Map<Result>(source: property);
            }
        }
    }
}