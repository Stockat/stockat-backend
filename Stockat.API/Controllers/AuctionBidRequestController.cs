using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices.IAuctionServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionBidRequestController : ControllerBase
    {
        private readonly IAuctionBidRequestService _bidService;

        public AuctionBidRequestController(IAuctionBidRequestService bidService)
        {
            _bidService = bidService;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionBidRequestDto>> CreateBid(AuctionBidRequestCreateDto dto)
        {
            var bid = await _bidService.CreateBidAsync(dto);

            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionBidRequestDto>> GetBid(int id)
        {
            var bid = await _bidService.GetBidByIdAsync(id);

            return bid != null ? Ok(bid) : NotFound();
        }

        [HttpGet("auction/{auctionId}")]
        public async Task<ActionResult<IEnumerable<AuctionBidRequestDto>>> GetBidsForAuction(int auctionId)
        {
            var bids = await _bidService.GetBidsByAuctionAsync(auctionId);

            return Ok(bids);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBid(int id)
        {
            await _bidService.DeleteBidAsync(id);

            return NoContent();
        }
    }
}
