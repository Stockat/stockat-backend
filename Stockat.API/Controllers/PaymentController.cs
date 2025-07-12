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
using Stockat.Core.DTOs.ServiceRequestDTOs;

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
            var endpointSecret = "whsec_998b0fe189fdd578f23438e132d9f7f2425b5982eb5f3ec3d98e14275daa0d3e";
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
                            await _serviceManager.OrderService.InvoiceGeneratorAsync(id);
                            break;
                        case "service_request":
                            await _serviceManager.ServiceRequestService.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            await _serviceManager.ServiceRequestService.InvoiceGeneratorAsync(id);
                            break;
                    }

                    break;

                case "checkout.session.expired": // The user did not complete the checkout (timed out or closed tab) within 24 hours
                case "payment_intent.payment_failed": // The payment was attempted but failed (e.g. card declined)
                    string failedId = stripeEvent.Type == "checkout.session.expired"
                        ? ((Session)stripeEvent.Data.Object).Id
                        : ((PaymentIntent)stripeEvent.Data.Object).Id;

                    var session2 = stripeEvent.Data.Object as Session;
                    _logger.LogDebug(session2.Id);
                    //var orderId2 = session2.Metadata["orderId"];
                    //var type2 = session2.Metadata["type"];
                    //// int id2 = int.Parse(orderId2);
                    //int id2 = 5;
                    var order = await _serviceManager.OrderService.getorderbySessionOrPaymentId(session2.Id);
                    if (order is not null)
                    {
                        _logger.LogDebug(session2.Id);
                        await _serviceManager.OrderService.UpdateStripePaymentID(order.Id, session2.Id, session2.PaymentIntentId);
                        await _serviceManager.OrderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled);
                    }
                    else
                    {
                        await _serviceManager.ServiceRequestService.CancelServiceRequestOnPaymentFailureAsync(session2.Id);
                    }
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
// Secret Key whsec_998b0fe189fdd578f23438e132d9f7f2425b5982eb5f3ec3d98e14275daa0d3e

