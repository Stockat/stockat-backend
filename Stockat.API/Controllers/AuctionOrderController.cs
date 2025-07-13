using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuctionOrderController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public AuctionOrderController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost("auction/{auctionId}")]
        public async Task<ActionResult<AuctionOrderDto>> CreateForClosedAuction(int auctionId)
        {
            try
            {
                var order = await _serviceManager.AuctionOrderService.CreateOrderForWinningBidAsync(auctionId);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _serviceManager.AuctionOrderService.UpdateOrderStatusAsync(orderId, dto.Status);
                return Ok(new { message = "Order status updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Order with ID {orderId} not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the order status.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionOrderDto>> GetOrder(int id)
        {
            var order = await _serviceManager.AuctionOrderService.GetOrderByIdAsync(id);
            return order != null ? Ok(order) : NotFound();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AuctionOrderDto>>> GetUserOrders(string userId)
        {
            var orders = await _serviceManager.AuctionOrderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        [HttpGet("auction/{auctionId}")]
        public async Task<ActionResult<AuctionOrderDto>> GetOrderByAuction(int auctionId)
        {
            try
            {
                var order = await _serviceManager.AuctionOrderService.GetOrderByAuctionIdAsync(auctionId);
                return order != null ? Ok(order) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("payment/{orderId}")]
        public async Task<IActionResult> ProcessPayment(int orderId, ProcessPaymentDto paymentDto)
        {
            try
            {
                await _serviceManager.AuctionOrderService.ProcessPaymentAsync(orderId, paymentDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("payment-failed/{orderId}")]
        public async Task<IActionResult> MarkPaymentFailed(int orderId, [FromBody] string reason = null)
        {
            try
            {
                await _serviceManager.AuctionOrderService.MarkPaymentFailedAsync(orderId, reason);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{orderId}/address-info")]
        public async Task<IActionResult> UpdateOrderAddressInfo(int orderId, [FromBody] AuctionOrderDto dto)
        {
            try
            {
                await _serviceManager.AuctionOrderService.UpdateOrderAddressInfoAsync(orderId, dto.ShippingAddress, dto.RecipientName, dto.PhoneNumber, dto.Notes);
                return Ok(new { message = "Order address info updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionOrderDto>>> GetAll()
        {
            var orders = await _serviceManager.AuctionOrderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpPost("{orderId:int}/checkout")]
        [Authorize]
        public async Task<IActionResult> CreateCheckoutSession(int orderId)
        {
            try
            {
                var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("User is not authenticated.");

                var result = await _serviceManager.AuctionOrderService.CreateStripeCheckoutSessionAsync(orderId, buyerId);
                return StatusCode(result.Status, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Checkout Internal server error: {ex.Message}");
            }
        }

        // Debug endpoint to manually complete payment (for testing)
        [HttpPost("{orderId:int}/complete-payment")]
        [Authorize]
        public async Task<IActionResult> CompletePaymentManually(int orderId)
        {
            try
            {
                var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(buyerId))
                    return Unauthorized("User is not authenticated.");

                // Simulate webhook completion
                await _serviceManager.AuctionOrderService.UpdateStripePaymentID(orderId, "test_session", "test_payment_intent");
                await _serviceManager.AuctionOrderService.HandleAuctionOrderCompletion(null, orderId.ToString());
                
                return Ok(new { message = "Payment completed manually for testing." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Manual completion error: {ex.Message}");
            }
        }
    }
}
