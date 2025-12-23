using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;
using ReSys.Shop.Core.Feature.Accounts.Auth.Internals;
using ReSys.Shop.Core.Feature.Accounts.Profile;

namespace ReSys.Shop.Core.Feature.Storefront.Account;

public static partial class AccountModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/account")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapPost(pattern: string.Empty, handler: async (
                    [FromBody] InternalModule.Register.Param param,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Register.Command(Param: param), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Account registered successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Register);

            group.MapGet(pattern: string.Empty, handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.GetProfile.Query(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Profile retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetProfile)
                .RequireAuthorization();

            group.MapPatch(pattern: string.Empty, handler: async (
                    [FromBody] ProfileModule.Update.Param param,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.UpdateProfile.Command(Param: param), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Profile updated successfully"));
                })
                .UseEndpointMeta(meta: Annotations.UpdateProfile)
                .RequireAuthorization();

            group.MapDelete(pattern: string.Empty, handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.DeleteAccount.Command(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseDeleted(message: "Account deleted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.DeleteAccount)
                .RequireAuthorization();
        }
    }
}
