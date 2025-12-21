using Carter;

using Microsoft.AspNetCore.Mvc;

using ReSys.Shop.Core.Domain.Settings.PaymentMethods;
using ReSys.Shop.Infrastructure.Payments.Gateways;

namespace ReSys.Shop.Api.Endpoints.Storefront;

public class WebhookModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/storefront/webhooks")
            .WithTags("Payment Webhooks");

        group.MapPost("stripe", async (
            HttpContext context, 
            [FromServices] PaymentWebhookProcessor processor,
            [FromServices] ILogger<WebhookModule> logger,
            CancellationToken ct) =>
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var signature = context.Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(signature))
            {
                logger.LogWarning("Stripe webhook received without signature.");
                return Results.BadRequest("Missing signature");
            }

            var result = await processor.ProcessWebhookAsync(
                PaymentMethod.PaymentType.Stripe,
                json,
                signature!,
                ct);

            if (result.IsError)
            {
                logger.LogError("Stripe webhook processing failed: {Error}", result.FirstError.Description);
                // Return 200 even on error if it's a domain error to stop Stripe from retrying indefinitely
                // but return 400 if it's a signature or critical failure.
                return result.FirstError.Type == ErrorOr.ErrorType.Unauthorized ? Results.BadRequest() : Results.Ok();
            }

            return Results.Ok();
        });

        group.MapPost("paypal", async (
            HttpContext context, 
            [FromServices] PaymentWebhookProcessor processor,
            [FromServices] ILogger<WebhookModule> logger,
            CancellationToken ct) =>
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var authAlgo = context.Request.Headers["PAYPAL-AUTH-ALGO"];
            var certUrl = context.Request.Headers["PAYPAL-CERT-URL"];
            var transmissionId = context.Request.Headers["PAYPAL-TRANSMISSION-ID"];
            var transmissionSig = context.Request.Headers["PAYPAL-TRANSMISSION-SIG"];
            var transmissionTime = context.Request.Headers["PAYPAL-TRANSMISSION-TIME"];

            // For PayPal, we combine these into a single signature string or pass them as a dictionary
            var signature = $"{transmissionId}|{transmissionSig}|{transmissionTime}|{authAlgo}|{certUrl}";

            var result = await processor.ProcessWebhookAsync(
                PaymentMethod.PaymentType.PayPal,
                json,
                signature,
                ct);

            return result.IsError ? Results.BadRequest() : Results.Ok();
        });
    }
}