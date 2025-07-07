using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs.StockDTOs;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceManager _serviceManager;
        public StockController(ILoggerManager logger, IServiceManager serviceManager)
        {
            _logger = logger;
            _serviceManager = serviceManager;
        }

        // Add Stock To Product
        [HttpPost]
        public async Task<IActionResult> AddStockAsync([FromBody] AddStockDTO stockDto)
        {
            if (stockDto == null)
            {
                _logger.LogError("AddStockAsync: Stock DTO is null.");
                return BadRequest("Full Stock data is required.");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("AddStockAsync: Invalid model state.");
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _serviceManager.StockService.AddStockAsync(stockDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddStockAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding stock.");
            }
        }


        // Get Stock By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogError("GetStockByIdAsync: Invalid stock ID.");
                return BadRequest("Invalid stock ID.");
            }
            try
            {
                var response = await _serviceManager.StockService.GetStockByIdAsync(id);
                if (response == null)
                {
                    _logger.LogError($"GetStockByIdAsync: Stock with ID {id} not found.");
                    return NotFound($"Stock with ID {id} not found.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetStockByIdAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving stock.");
            }
        }

        // Get All Stocks
        [HttpGet("all")]
        public async Task<IActionResult> GetAllStocksAsync()
        {
            try
            {
                var response = await _serviceManager.StockService.GetAllStocksAsync();
                if (response == null || response.Data == null || !response.Data.Any())
                {
                    _logger.LogError("GetAllStocksAsync: No stocks found.");
                    return NotFound("No stocks found.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllStocksAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving stocks.");
            }
        }

        // Get All Stocks For a Specific Product
        [HttpGet("for-product/{productId}")]
        public async Task<IActionResult> GetStocksForProductAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogError("GetStocksForProductAsync: Invalid product ID.");
                return BadRequest("Invalid product ID.");
            }
            try
            {
                var response = await _serviceManager.StockService.GetStocksByProductIdAsync(productId);
                if (response == null || response.Data == null || !response.Data.Any())
                {
                    _logger.LogError($"GetStocksForProductAsync: No stocks found for product ID {productId}.");
                    return NotFound($"No stocks found for product ID {productId}.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetStocksForProductAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving stocks for the product.");
            }
        }
        

        // Update Stock
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStockAsync(int id, [FromBody] UpdateStockDTO stockDto)
        {
            if (stockDto == null)
            {
                _logger.LogError("UpdateStockAsync: Stock DTO is null.");
                return BadRequest("Stock update data is required.");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("UpdateStockAsync: Invalid model state.");
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _serviceManager.StockService.UpdateStockAsync(id, stockDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateStockAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating stock.");
            }
        }

        // Delete Stock
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogError("DeleteStockAsync: Invalid stock ID.");
                return BadRequest("Invalid stock ID.");
            }
            try
            {
                var response = await _serviceManager.StockService.DeleteStockAsync(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteStockAsync: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting stock.");
            }
        }
    }
}
