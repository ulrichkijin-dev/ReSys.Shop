namespace ReSys.Shop.Core.Common.Services.Security.Authentication.Contexts;

public interface IUserContext
{
    string? UserId { get; }
    string? AdhocCustomerId { get; } 
    Guid? StoreId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    void SetAdhocCustomerId(string adhocCustomerId); 
    
}
