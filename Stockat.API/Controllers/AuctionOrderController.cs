using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices.IAuctionServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionOrderController : ControllerBase
    {
        private readonly IAuctionOrderService _orderService;

        public AuctionOrderController(IAuctionOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("auction/{auctionId}")]
        public async Task<ActionResult<AuctionOrderDto>> CreateForClosedAuction(int auctionId)
        {
            try
            {
                var order = await _orderService.CreateOrderForWinningBidAsync(auctionId);

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
            var order = await _orderService.GetOrderByIdAsync(id);

            return order != null ? Ok(order) : NotFound();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AuctionOrderDto>>> GetUserOrders(string userId)
        {
            var orders = await _orderService.GetOrdersByUserAsync(userId);

            return Ok(orders);
        }

        [HttpGet("auction/{auctionId}")]
        public async Task<ActionResult<AuctionOrderDto>> GetOrderByAuction(int auctionId)
        {
            try
            {
                var order = await _orderService.GetOrderByAuctionIdAsync(auctionId);

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
                await _orderService.ProcessPaymentAsync(orderId, paymentDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
