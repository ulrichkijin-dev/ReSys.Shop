using MapsterMapper;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace  ReSys.Shop.Core.Feature.Admin.Settings.PaymentMethods;

public static partial class PaymentMethodModule
{
    public static class Update
    {
        public record Request : Models.Parameter;
        public record Result : Models.Detail;
        public sealed record Command(Guid Id, Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id).NotEmpty();
                RuleFor(expression: x => x.Request)
                    .SetValidator(new Models.ParameterValidator());
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
                var request = command.Request;
                var paymentMethod = await applicationDbContext.Set<PaymentMethod>()
                    .FindAsync(keyValues: [command.Id], cancellationToken: ct);
                if (paymentMethod == null)
                    return PaymentMethod.Errors.NotFound(id: command.Id);

                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim() != paymentMethod.Name)
                {
                    var uniqueNameCheck = await applicationDbContext.Set<PaymentMethod>()
                        .Where(predicate: m => m.Id != paymentMethod.Id)
                        .CheckNameIsUniqueAsync<PaymentMethod, Guid>(name: request.Name, prefix: nameof(PaymentMethod), cancellationToken: ct);
                    if (uniqueNameCheck.IsError)
                        return uniqueNameCheck.Errors;
                }

                await applicationDbContext.BeginTransactionAsync(cancellationToken: ct);

                var encryptedPrivateMetadata = request.PrivateMetadata?
                    .ToDictionary(
                        keySelector: entry => entry.Key,
                        elementSelector: entry =>
                            entry.Value != null ? (object?)encryptor.Encrypt(entry.Value.ToString()!) : null);

                var updateResult = paymentMethod.Update(
                    name: request.Name,
                    presentation: request.Presentation,
                    description: request.Description,
                    active: request.Active,
                    autoCapture: request.AutoCapture,
                    position: request.Position,
                    displayOn: request.DisplayOn,
                    publicMetadata: request.PublicMetadata,
                    privateMetadata: encryptedPrivateMetadata);

                if (updateResult.IsError) return updateResult.Errors;

                await applicationDbContext.SaveChangesAsync(cancellationToken: ct);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: ct);

                return mapper.Map<Result>(source: updateResult.Value);
            }
        }
    }
}