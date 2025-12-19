using System.Text.Json.Serialization;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Claims;

public record AuthorizeClaimData(
    [property: JsonPropertyName(name: "user_id")] string UserId,
    [property: JsonPropertyName(name: "user_name")] string UserName,
    [property: JsonPropertyName(name: "email")] string Email,
    [property: JsonPropertyName(name: "permissions")] IReadOnlyList<string> Permissions,
    [property: JsonPropertyName(name: "roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName(name: "policies")] IReadOnlyList<string> Policies
);
