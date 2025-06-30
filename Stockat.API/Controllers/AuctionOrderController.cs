using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
