using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Location.Countries; // For Country
using ReSys.Shop.Core.Domain.Location.States;    // For State


namespace  ReSys.Shop.Core.Feature.Admin.Inventories.StockLocations;

public static partial class StockLocationModule
{
    public static class Create
    {
        public sealed record Request : Models.Parameter; // Renamed from Param
        public sealed record Result : Models.ListItem;

        public sealed record Command(Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Request).SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var uniqueNameCheck = await applicationDbContext.Set<StockLocation>()
                    .CheckNameIsUniqueAsync<StockLocation, Guid>(name: param.Name, prefix: nameof(StockLocation),
                        cancellationToken: ct);
                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                // Validate Country and State if provided
                if (param.CountryId.HasValue)
                {
                    var country = await applicationDbContext.Set<Country>()
                        .FindAsync(keyValues: [param.CountryId.Value], cancellationToken: ct);
                    if (country == null)
                        return Error.NotFound("Country.NotFound",
                            $"Country with ID '{param.CountryId.Value}' was not found.");
                }

                if (param.StateId.HasValue)
                {
                    var state = await applicationDbContext.Set<State>()
                        .FindAsync(keyValues: [param.StateId.Value], cancellationToken: ct);
                    if (state == null)
                        return Error.NotFound("State.NotFound",
                            $"State with ID '{param.StateId.Value}' was not found.");
                }

                var createResult = StockLocation.Create(
                    name: param.Name,
                    presentation: param.Presentation,
                    active: param.Active,
                    isDefault: param.Default,
                    countryId: param.CountryId,
                    address1: param.Address1,
                    address2: param.Address2,
                    city: param.City,
                    zipcode: param.ZipCode,
                    stateId: param.StateId,
                    phone: param.Phone,
                    company: param.Company,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: param.PrivateMetadata);

                if (createResult.IsError) return createResult.Errors;

                applicationDbContext.Set<StockLocation>().Add(entity: createResult.Value);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: createResult.Value);
            }
        }
    }
}
