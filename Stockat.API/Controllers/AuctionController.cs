using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.Entities;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices.IAuctionServices;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Stockat.Core.DTOs.AuctionDTOs;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionsController> _logger;
        private readonly IMapper _mapper;

        public AuctionsController(IAuctionService auctionService,
            ILogger<AuctionsController> logger,
            IMapper mapper)
        {
            _auctionService = auctionService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDetailsDto>> CreateAuction([FromBody] AuctionCreateDto createDto)
        {
            var auction = _mapper.Map<Auction>(createDto);
            if (auction == null)
                throw new Exception("Mapping faild");

                var result = await _auctionService.AddAuctionAsync(auction);
                return CreatedAtAction(nameof(GetAuctionById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDetailsDto>> UpdateAuction(int id, AuctionUpdateDto updateDto)
        {
            try
            {
                var auction = _mapper.Map<Auction>(updateDto);
                auction.Id = id;
                var result = await _auctionService.EditAuctionAsync(id, updateDto);

                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating auction {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction(int id)
        {
            try
            {
                await _auctionService.RemoveAuctionAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting auction {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDetailsDto>> GetAuctionById(int id)
        {
            try
            {
                var auction = await _auctionService.GetAuctionDetailsAsync(id);
                return Ok(auction);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (IdParametersBadRequestException)
            {
                return BadRequest("Invalid Id");
            }
        }

        [HttpGet("GetAllAuctions")]
        public async Task<ActionResult<PagedResponse<AuctionDetailsDto>>> GetAllAuctions(string status = null, string sellerId = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                Expression<Func<Auction, bool>> filter = auction => true;

                if (!string.IsNullOrEmpty(status))
                {
                    var now = DateTime.UtcNow;
                    filter = status.ToLower() switch
                    {
                        "upcoming" => auction => auction.StartTime > now,
                        "active" => auction => auction.StartTime <= now && auction.EndTime > now,
                        "ended" => auction => auction.EndTime <= now,
                        _ => filter
                    };
                }

                //add seller filter
                if (!string.IsNullOrEmpty(sellerId))
                {
                    Expression<Func<Auction, bool>> sellerFilter = a => a.SellerId == sellerId;

                    filter = filter.And(sellerFilter);
                }

                var totalCount = await _auctionService.GetAuctionCountAsync(filter);

                var auctions = await _auctionService.SearchAuctionsAsync(filter, skip: (pageNumber - 1) * pageSize, take: pageSize, orderBy: a => a.StartTime, orderByDirection: "DESC");

                var response = new PagedResponse<AuctionDetailsDto>( auctions.ToList(), pageNumber, pageSize, totalCount);

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("GetAllAuctionsBasic")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AuctionDetailsDto>>> GetAllAuctions()
        {
            try
            {
                var auctions = await _auctionService.GetAllAuctionsAsync();

                if (!auctions.Any()) return NotFound("No auctions found");

                return Ok(auctions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "ثrror occurred");
            }
        }

        [HttpGet("Query")]
        public async Task<ActionResult<IEnumerable<AuctionDetailsDto>>> QueryAuctions( string? name, int? productId, string? sellerId,
               DateTime? startDateFrom, DateTime? startDateTo, bool? isClosed)
        {
            try
            {
                Expression<Func<Auction, bool>> filter = auction =>
                    (string.IsNullOrEmpty(name) || auction.Name.Contains(name)) &&
                    (!productId.HasValue || auction.ProductId == productId.Value) &&
                    (string.IsNullOrEmpty(sellerId) || auction.SellerId == sellerId) &&
                    (!startDateFrom.HasValue || auction.StartTime >= startDateFrom.Value) &&
                    (!startDateTo.HasValue || auction.StartTime <= startDateTo.Value) &&
                    (!isClosed.HasValue || auction.IsClosed == isClosed.Value);

                var auctions = await _auctionService.QueryAuctionsAsync(filter);
                return Ok(auctions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (NullObjectParameterException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        /// Searches auctions with custom criteria-> paged response
        [HttpPost("search")]
        public async Task<ActionResult<PagedResponse<AuctionDetailsDto>>> SearchAuctions(
            [FromBody] AuctionSearchDto searchDto)
        {
            try
            {
                // Build complex filter from DTO
                Expression<Func<Auction, bool>> filter = auction => true;

                if (!string.IsNullOrEmpty(searchDto.Name))
                {
                    filter = filter.And(a => a.Name.Contains(searchDto.Name));
                }

                if (searchDto.MinPrice.HasValue)
                {
                    filter = filter.And(a => a.StartingPrice >= searchDto.MinPrice.Value);
                }

                if (searchDto.MaxPrice.HasValue)
                {
                    filter = filter.And(a => a.StartingPrice <= searchDto.MaxPrice.Value);
                }

                if (searchDto.StartDateFrom.HasValue)
                {
                    filter = filter.And(a => a.StartTime >= searchDto.StartDateFrom.Value);
                }

                if (searchDto.StartDateTo.HasValue)
                {
                    filter = filter.And(a => a.StartTime <= searchDto.StartDateTo.Value);
                }

                var totalCount = await _auctionService.GetAuctionCountAsync(filter);
                var auctions = await _auctionService.SearchAuctionsAsync(
                    filter,
                    skip: (searchDto.PageNumber - 1) * searchDto.PageSize,
                    take: searchDto.PageSize,
                    orderBy: GetOrderByExpression(searchDto.SortBy),
                    orderByDirection: searchDto.SortDirection);

                var response = new PagedResponse<AuctionDetailsDto>(
                    auctions.ToList(),
                    searchDto.PageNumber,
                    searchDto.PageSize,
                    totalCount);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching auctions");
                return BadRequest("Invalid search parameters");
            }
        }

        private Expression<Func<Auction, object>> GetOrderByExpression(string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "name" => a => a.Name,
                "startdate" => a => a.StartTime,
                "enddate" => a => a.EndTime,
                "price" => a => a.StartingPrice,
                _ => a => a.StartTime
            };
        }
    }

   
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<T> Data { get; set; }

        public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalCount)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }

   
    // Expression helper
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(left, param),
                Expression.Invoke(right, param)
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

}
