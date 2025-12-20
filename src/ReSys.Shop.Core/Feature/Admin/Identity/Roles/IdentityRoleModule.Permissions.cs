using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using ReSys.Shop.Core.Common.Services.Security.Authorization.Claims;
using ReSys.Shop.Core.Domain.Identity.Roles;



namespace  ReSys.Shop.Core.Feature.Admin.Identity.Roles;

public static partial class IdentityRoleModule
{
    public static partial class Permissions
    {
        public static class Assign
        {
            public sealed record Request(string ClaimValue);

            public sealed record Command(string RoleId, Request Request) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(expression: x => x.RoleId)
                        .NotEmpty()
                        .WithMessage(errorMessage: "Role ID is required.");
                    RuleFor(expression: x => x.Request.ClaimValue)
                        .NotEmpty()
                        .WithMessage(errorMessage: "Claim value is required.");
                }
            }

            public sealed class CommandHandler(
                RoleManager<Role> roleManager,
                IApplicationDbContext applicationDbContext,
                ILogger<CommandHandler> logger
            ) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
                {
                    try
                    {
                        // Check if role exists
                        Role? role = await roleManager.FindByIdAsync(command.RoleId);
                        if (role == null)
                        {
                            return Role.Errors.RoleNotFound;
                        }

                        // Create claim object
                        var claim = new System.Security.Claims.Claim(CustomClaim.Permission,
                            command.Request.ClaimValue);

                        // Check if claim already exists for the role
                        IList<System.Security.Claims.Claim> existingClaims = await roleManager.GetClaimsAsync(role);
                        if (existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                        {
                            return Error.Conflict(code: "Role.PermissionAlreadyAssigned",
                                description: $"Permission '{claim.Value}' is already assigned to role '{role.Name}'.");
                        }

                        await applicationDbContext.BeginTransactionAsync(cancellationToken: cancellationToken);

                        // Add claim to role
                        IdentityResult result = await roleManager.AddClaimAsync(role, claim);
                        if (!result.Succeeded)
                        {
                            string errors = string.Join(separator: "; ",
                                values: result.Errors.Select(selector: e => e.Description));
                            logger.LogError(
                                message: "Failed to assign permission {ClaimValue} to role {RoleId}: {Errors}",
                                args: [command.Request.ClaimValue, command.RoleId, errors]);

                            return Error.Failure(code: "Role.PermissionAssignmentFailed",
                                description: $"Failed to assign permission to role: {errors}");
                        }

                        await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                        await applicationDbContext.CommitTransactionAsync(cancellationToken: cancellationToken);

                        logger.LogInformation(
                            message: "Successfully assigned permission {ClaimValue} to role {RoleId}",
                            args: [command.Request.ClaimValue, command.RoleId]);

                        return Result.Success;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(exception: ex,
                            message: "Unexpected error assigning permission {ClaimValue} to role {RoleId}",
                            args: [command.Request.ClaimValue, command.RoleId]);
                        return Role.Errors.UnexpectedError(operation: "assigning permission to role");
                    }
                }
            }
        }
    }
}