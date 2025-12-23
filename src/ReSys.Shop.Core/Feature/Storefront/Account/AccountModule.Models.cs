using Mapster;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Feature.Storefront.Account;

public static partial class AccountModule
{
    public static class Models
    {
        public record AccountDetail
        {
            public Guid Id { get; init; }
            public string Email { get; init; } = string.Empty;
            public string? UserName { get; init; }
            public string? FirstName { get; init; }
            public string? LastName { get; init; }
            public string? PhoneNumber { get; init; }
            public DateTimeOffset? DateOfBirth { get; init; }
        }

        public sealed class Mapping : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<User, AccountDetail>();
            }
        }
    }
}
