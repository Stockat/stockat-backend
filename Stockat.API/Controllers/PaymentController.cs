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
        _logger.LogInfo($"Webhook received: {json}");

        try
        {
            var endpointSecret = "whsec_r7K1sJ3doUtYevHw1RQCd0WDDagzHab7";
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret
            );
            string sessionId = null;
            string paymentIntentId = null;

            
            _logger.LogInfo($"Stripe event type: {stripeEvent.Type}");


            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    var orderId = session.Metadata["orderId"];
                    var type = session.Metadata["type"];
                    int id = int.Parse(orderId);
                    
                    _logger.LogInfo($"Checkout session completed. OrderId: {orderId}, Type: {type}, SessionId: {session.Id}");


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
                        case "auction_order":
                            _logger.LogInfo($"Auction order checkout complete. Order ID: {orderId}, Session ID: {session.Id}, Payment Intent ID: {session.PaymentIntentId}");
                            await _serviceManager.AuctionOrderService.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            await _serviceManager.AuctionOrderService.HandleAuctionOrderCompletion(session, orderId);
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

                    var session2 = stripeEvent.Data.Object as Session;
                    string orderId2 = null;
                    string type2 = null;
                    int id2 = 0;
                    
                    if (session2 != null)
                    {
                        orderId2 = session2.Metadata["orderId"];
                        type2 = session2.Metadata["type"];
                        id2 = int.Parse(orderId2);
                    }

                    switch (type2)
                    {
                        case "order":
                        case "req":
                            await _serviceManager.OrderService.UpdateStripePaymentID(id2, session2.Id, session2.PaymentIntentId);
                            await _serviceManager.OrderService.UpdateStatus(id2, OrderStatus.Cancelled, PaymentStatus.Failed);
                            break;
                        case "service_request":
                            await _serviceManager.ServiceRequestService.CancelServiceRequestOnPaymentFailureAsync(session2.Id);
                            break;
                        case "auction_order":
                            //_logger.LogInfo($"Auction order checkout complete. Order ID: {orderId}, Session ID: {session.Id}, Payment Intent ID: {session.PaymentIntentId}");
    
                            // First update payment IDs
                            await _serviceManager.AuctionOrderService.UpdateStripePaymentID(id2, session2.Id, session2.PaymentIntentId);
                            
                            // Then update payment status and order status
                            await _serviceManager.AuctionOrderService.UpdateOrderPaymentStatus(id2, PaymentStatus.Paid);
                            await _serviceManager.AuctionOrderService.UpdateOrderStatusAsync(id2, OrderStatus.Processing);
                            
                            _logger.LogInfo($"Auction order {id2} payment completed successfully. Status: Processing, PaymentStatus: Paid");
                             break;
                            
                    }

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


    // Test endpoint to verify webhook is working
    [HttpPost("webhook/test")]
    [AllowAnonymous]
    public IActionResult TestWebhook()
    {
        _logger.LogInfo("Test webhook endpoint called");
        return Ok(new { message = "Webhook test endpoint is working" });
    }

    // End Stripe Endpoints

}

// stripe listen --forward-to http://localhost:5250/api/Payment/webhook/confirm --skip-verify
//
// Secret Key whsec_998b0fe189fdd578f23438e132d9f7f2425b5982eb5f3ec3d98e14275daa0d3e

