using Carter;

namespace ReSys.Shop.Api.Endpoints.Storefront;

public class HelloWorldEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/hello", () => "Hello World from ReSys.Shop API!");
    }
}
