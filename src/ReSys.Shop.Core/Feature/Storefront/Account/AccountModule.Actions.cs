using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using ReSys.Shop.Core.Domain.Identity.Users;
using ReSys.Shop.Core.Feature.Accounts.Auth.Internals;
using ReSys.Shop.Core.Feature.Accounts.Profile;
using ReSys.Shop.Core.Feature.Accounts.Common;

namespace ReSys.Shop.Core.Feature.Storefront.Account;

public static partial class AccountModule
{
    public static class Actions
    {
        public static class Register
        {
            public sealed record Command(InternalModule.Register.Param Param) : ICommand<Models.AccountDetail>;

            public sealed class CommandHandler(ISender mediator, IMapper mapper)
                : ICommandHandler<Command, Models.AccountDetail>
            {
                public async Task<ErrorOr<Models.AccountDetail>> Handle(Command command, CancellationToken ct)
                {
                    var result = await mediator.Send(new InternalModule.Register.Command(command.Param), ct);
                    if (result.IsError) return result.Errors;

                    return mapper.Map<Models.AccountDetail>(result.Value);
                }
            }
        }

        public static class GetProfile
        {
            public sealed record Query : IQuery<Models.AccountDetail>;

            public sealed class QueryHandler(IUserContext userContext, UserManager<User> userManager, IMapper mapper)
                : IQueryHandler<Query, Models.AccountDetail>
            {
                public async Task<ErrorOr<Models.AccountDetail>> Handle(Query request, CancellationToken ct)
                {
                    if (userContext.UserId == null) return Error.Unauthorized();

                    var user = await userManager.FindByIdAsync(userContext.UserId);
                    if (user == null) return User.Errors.NotFound(userContext.UserId);

                    return mapper.Map<Models.AccountDetail>(user);
                }
            }
        }

        public static class UpdateProfile
        {
            public sealed record Command(ProfileModule.Update.Param Param) : ICommand<Models.AccountDetail>;

            public sealed class CommandHandler(ISender mediator, IUserContext userContext)
                : ICommandHandler<Command, Models.AccountDetail>
            {
                public async Task<ErrorOr<Models.AccountDetail>> Handle(Command command, CancellationToken ct)
                {
                    var result = await mediator.Send(new ProfileModule.Update.Command(userContext.UserId, command.Param), ct);
                    if (result.IsError) return result.Errors;

                    return await mediator.Send(new GetProfile.Query(), ct);
                }
            }
        }

        public static class DeleteAccount
        {
            public sealed record Command : ICommand<Deleted>;

            public sealed class CommandHandler(IUserContext userContext, UserManager<User> userManager)
                : ICommandHandler<Command, Deleted>
            {
                public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
                {
                    if (userContext.UserId == null) return Error.Unauthorized();

                    var user = await userManager.FindByIdAsync(userContext.UserId);
                    if (user == null) return User.Errors.NotFound(userContext.UserId);

                    var result = await userManager.DeleteAsync(user);
                    if (!result.Succeeded) return result.Errors.ToApplicationResult();

                    return Result.Deleted;
                }
            }
        }
    }
}
