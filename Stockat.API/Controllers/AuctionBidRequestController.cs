using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionBidRequestController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public AuctionBidRequestController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionBidRequestDto>> CreateBid(AuctionBidRequestCreateDto dto)
        {
            var bid = await _serviceManager.AuctionBidRequestService.CreateBidAsync(dto);
            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionBidRequestDto>> GetBid(int id)
        {
            var bid = await _serviceManager.AuctionBidRequestService.GetBidByIdAsync(id);
            return bid != null ? Ok(bid) : NotFound();
        }

        [HttpGet("auction/{auctionId}")]
        public async Task<ActionResult<IEnumerable<AuctionBidRequestDto>>> GetBidsForAuction(int auctionId)
        {
            try
            {
                var bids = await _serviceManager.AuctionBidRequestService.GetBidsByAuctionAsync(auctionId);
                return Ok(bids);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBidsByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");

            var bids = await _serviceManager.AuctionBidRequestService.GetBidsByUserIdAsync(userId);

            if (bids == null || !bids.Any())
                return NotFound($"No bids found for user with ID: {userId}");

            return Ok(bids);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBid(int id)
        {
            await _serviceManager.AuctionBidRequestService.DeleteBidAsync(id);
            return NoContent();
        }
    }
}
