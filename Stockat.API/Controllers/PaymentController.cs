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
            string sessionId = null;
            string paymentIntentId = null;

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

                    //case "checkout.session.expired": // The user did not complete the checkout (timed out or closed tab) within 24 hours
                    //    var session2 = (Session)stripeEvent.Data.Object;
                    //    _logger.LogDebug("********************************************");
                    //    _logger.LogDebug("Session ID:" + session2.Id);
                    //    _logger.LogDebug("********************************************");
                    //    sessionId = session2.Id;
                    //    paymentIntentId = session2.PaymentIntentId;

                    //    // Continue using session logic
                    //    var order = await _serviceManager.OrderService.getorderbySessionOrPaymentId(sessionId);
                    //    if (order != null)
                    //    {
                    //        await _serviceManager.OrderService.UpdateStripePaymentID(order.Id, session2.Id, session2.PaymentIntentId);
                    //        await _serviceManager.OrderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled);
                    //    }
                    //    else
                    //    {
                    //        await _serviceManager.ServiceRequestService.CancelServiceRequestOnPaymentFailureAsync(sessionId);
                    //    }
                    //    break;
                    //case "payment_intent.payment_failed": // The payment was attempted but failed (e.g. card declined)
                    //    var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                    //    paymentIntentId = paymentIntent.Id;

                    //    // Look up order by PaymentIntent
                    //    var order2 = await _serviceManager.OrderService.getorderbySessionOrPaymentId(paymentIntentId);
                    //    if (order2 != null)
                    //    {
                    //        await _serviceManager.OrderService.UpdateOrderStatusAsync(order2.Id, OrderStatus.Cancelled);
                    //    }
                    //    else
                    //    {
                    //        await _serviceManager.ServiceRequestService.CancelServiceRequestOnPaymentFailureAsync(paymentIntentId);
                    //    }
                    //    break;

                    //    break;

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

