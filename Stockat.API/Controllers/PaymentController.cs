using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.IServices;
using Stockat.Core;
using Microsoft.AspNetCore.Authorization;
using Stockat.Core.Enums;
using Stripe;
using Stripe.Checkout;
using Stockat.Core.Entities;
using System.Net.Mail;
using Stockat.Core.Helpers;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;


    public PaymentController(ILoggerManager logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    // Internal Stripe Func
    [HttpPost("webhook/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var endpointSecret = "whsec_bc6ecd5524ac3fc58389066e50b5b26a5147f9ea96f8d7cec244370d5eb4c178";
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret
            );


            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    var orderId = session.Metadata["orderId"];
                    var type = session.Metadata["type"];
                    int id = int.Parse(orderId);


                    switch (type)
                    {
                        case "order":
                            _logger.LogInfo($"Checkout complete. Order ID: {orderId}");

                            // TODO: Confirm the order in DB
                            await _serviceManager.OrderService.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            await _serviceManager.OrderService.UpdateStatus(id, OrderStatus.Processing, PaymentStatus.Paid);
                            await _serviceManager.OrderService.InvoiceGeneratorAsync(id);
                            break;
                        case "req":
                            await _serviceManager.OrderService.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            await _serviceManager.OrderService.UpdateStatus(id, OrderStatus.Processing, PaymentStatus.Paid);
                            break;
                    }

                    break;

                case "checkout.session.expired": // The user did not complete the checkout (timed out or closed tab) within 24 hours
                case "payment_intent.payment_failed": // The payment was attempted but failed (e.g. card declined)
                    string failedId = stripeEvent.Type == "checkout.session.expired"
                        ? ((Session)stripeEvent.Data.Object).Id
                        : ((PaymentIntent)stripeEvent.Data.Object).Id;

                    var session2 = stripeEvent.Data.Object as Session;
                    var orderId2 = session2.Metadata["orderId"];

                    int id2 = int.Parse(orderId2);

                    await _serviceManager.OrderService.UpdateStripePaymentID(id2, session2.Id, session2.PaymentIntentId);
                    await _serviceManager.OrderService.UpdateStatus(id2, OrderStatus.Cancelled, PaymentStatus.Failed);

                    _logger.LogWarn($"❌ Checkout failed/expired: {failedId}");
                    _logger.LogError("*************************************************************" + failedId);
                    // Mark order as Cancelled or Failed (same logic)
                    break;

                default:
                    _logger.LogError($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError($"Stripe webhook error: {e.Message}");
            return BadRequest();
        }
    }


    // End Stripe Endpoints

}

// stripe listen --forward-to http://localhost:5250/api/Payment/webhook/confirm --skip-verify
//
// Secret Key whsec_bc6ecd5524ac3fc58389066e50b5b26a5147f9ea96f8d7cec244370d5eb4c178

