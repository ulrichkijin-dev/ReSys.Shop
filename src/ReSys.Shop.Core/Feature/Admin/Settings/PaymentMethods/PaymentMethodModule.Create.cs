using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace  ReSys.Shop.Core.Feature.Admin.Settings.PaymentMethods;

public static partial class PaymentMethodModule
{
    public static class Create
    {
        public sealed record Request : Models.Parameter;
        public sealed record Result : Models.Detail;
        public sealed record Command(Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Request).SetValidator(validator: new Models.ParameterValidator());
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            IMapper mapper,
            ICredentialEncryptor encryptor)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var param = command.Request;
                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var uniqueNameCheck = await applicationDbContext.Set<PaymentMethod>()
                    .CheckNameIsUniqueAsync<PaymentMethod, Guid>(name: param.Name, prefix: nameof(PaymentMethod), cancellationToken: ct);
                if (uniqueNameCheck.IsError)
                    return uniqueNameCheck.Errors;

                var encryptedPrivateMetadata = param.PrivateMetadata?
                    .ToDictionary(
                        keySelector: entry => entry.Key,
                        elementSelector: entry =>
                            entry.Value != null ? (object?)encryptor.Encrypt(entry.Value.ToString()!) : null);

                var createResult = PaymentMethod.Create(
                    name: param.Name,
                    presentation: param.Presentation,
                    type: param.Type,
                    description: param.Description,
                    active: param.Active,
                    autoCapture: param.AutoCapture,
                    position: param.Position,
                    displayOn: param.DisplayOn,
                    publicMetadata: param.PublicMetadata,
                    privateMetadata: encryptedPrivateMetadata);

                if (createResult.IsError) return createResult.Errors;

                applicationDbContext.Set<PaymentMethod>().Add(entity: createResult.Value);
                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: createResult.Value);
            }
        }
    }
}