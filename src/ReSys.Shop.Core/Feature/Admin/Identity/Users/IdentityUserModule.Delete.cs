using Microsoft.AspNetCore.Identity;

using ReSys.Shop.Core.Domain.Identity.Users;


namespace  ReSys.Shop.Core.Feature.Admin.Identity.Users;

public static partial class IdentityUserModule
{
    public static class Delete
    {
        public sealed record Command(string Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(expression: x => x.Id)
                    .NotEmpty()
                    .WithMessage(errorMessage: "User ID is required.");
            }
        }

        public sealed class CommandHandler(
            IApplicationDbContext applicationDbContext,
            UserManager<User> userManager
        ) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
            {
                // Fetch:
                var user = await userManager.FindByIdAsync(command.Id);

                // Check: existence
                if (user == null)
                    return User.Errors.NotFound(command.Id);

                // Check: deletable (business logic from User aggregate)
                var deleteResult = user.Delete();
                if (deleteResult.IsError) return deleteResult.Errors;

                await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);

                var identityResult = await userManager.DeleteAsync(user);
                if (!identityResult.Succeeded)
                {
                    var errors = identityResult.Errors.Select(e => Error.Validation(code: e.Code, description: e.Description)).ToList();
                    return errors;
                }

                await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                return Result.Deleted;
            }
        }
    }
}