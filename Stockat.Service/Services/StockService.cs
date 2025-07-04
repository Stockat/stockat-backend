using System;
using Stockat.Core.IServices;
using Stockat.Core;
using Stockat.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.StockDTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Stockat.Core.Exceptions;

namespace Stockat.Service.Services
{
    public class StockService : IStockService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StockService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GenericResponseDto<AddStockDTO>> AddStockAsync(AddStockDTO stockDto)
        {
            // Check if the product exists and verify ownership
            var product = await _repo.ProductRepository.FindAsync(p => p.Id == stockDto.ProductId);
            if (product == null)
            {
                _logger.LogError($"Product with ID {stockDto.ProductId} not found.");
                throw new NotFoundException($"Product with ID {stockDto.ProductId} not found.");
            }

            // Check if the current user is the owner of the product
            //var currentUserId = GetCurrentUserId();
            //if (product.SellerId != currentUserId)
            //{
            //    _logger.LogError($"Unauthorized access: User {currentUserId} does not own product {stockDto.ProductId}.");
            //    throw new UnauthorizedAccessException("You do not own this product.");
            //}

            // Create stock entity from DTO
            var stock = _mapper.Map<Stock>(stockDto);

            // Map and add stock details
            foreach (var detailDto in stockDto.StockDetails)
            {
                var stockDetailsEntity = _mapper.Map<StockDetails>(detailDto);
                stock.StockDetails.Add(stockDetailsEntity);
            }

            // Add stock to the repository
            await _repo.StockRepo.AddAsync(stock);
            // Save changes to the database
            await _repo.CompleteAsync();

            return new GenericResponseDto<AddStockDTO>
            {
                Message = "Stock added successfully",
                Status = 201,
                Data = stockDto
            };

        }


        // Update Stock
        public async Task<GenericResponseDto<StockDTO>> UpdateStockAsync(int id, UpdateStockDTO stockDto)
        {
            // Get the existing stock with its product and details
            var existingStock = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] { "Product", "StockDetails" }
            );

            if (existingStock == null)
            {
                _logger.LogError($"Stock with ID {id} not found.");
                throw new NotFoundException($"Stock with ID {id} not found.");
            }

            // Check if the current user owns the product
            //var currentUserId = GetCurrentUserId();
            //if (existingStock.Product.SellerId != currentUserId)
            //{
            //    _logger.LogError($"User {currentUserId} does not own the product associated with stock {id}");
            //    throw new UnauthorizedAccessException("You do not own this product's stock");
            //}

            // Get the existing stock details to delete
            var existingDetails = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] { "StockDetails" }
            );

            // Remove existing stock details through the repository
            foreach (var detail in existingDetails.StockDetails.ToList())
            {
                _repo.StockDetailsRepo.Delete(detail);
            }

            // Add new stock details
            foreach (var detailDto in stockDto.StockDetails)
            {
                var stockDetail = new StockDetails
                {
                    StockId = existingStock.Id,
                    FeatureId = detailDto.FeatureId,
                    FeatureValueId = detailDto.FeatureValueId
                };
                await _repo.StockDetailsRepo.AddAsync(stockDetail);
            }

            // Update the stock quantity
            existingStock.Quantity = stockDto.Quantity;
            _repo.StockRepo.Update(existingStock);
            
            await _repo.CompleteAsync();

            // Get the updated stock with all related data for the response
            var updatedStock = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] {
                    "StockDetails",
                    "StockDetails.Feature",
                    "StockDetails.FeatureValue",
                    "Product"
                }
            );

            var resStockDto = _mapper.Map<StockDTO>(updatedStock);

            return new GenericResponseDto<StockDTO>
            {
                Data = resStockDto,
                Message = "Stock updated successfully",
                Status = 200
            };
        }

        // Delete Stock
        public async Task<GenericResponseDto<StockDTO>> DeleteStockAsync(int id)
        {
            // Get the stock with its product
            var stock = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] { "Product", "StockDetails" }
            );

            if (stock == null)
            {
                _logger.LogError($"Stock with ID {id} not found.");
                throw new NotFoundException($"Stock with ID {id} not found.");
            }

            // Check if the current user owns the product
            //var currentUserId = GetCurrentUserId();
            //if (stock.Product.SellerId != currentUserId)
            //{
            //    _logger.LogError($"User {currentUserId} does not own the product associated with stock {id}");
            //    throw new UnauthorizedAccessException("You do not own this product's stock");
            //}

            // Get the existing stock details to delete
            var existingDetails = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] { "StockDetails" }
            );

            // Remove existing stock details through the repository
            foreach (var detail in existingDetails.StockDetails.ToList())
            {
                _repo.StockDetailsRepo.Delete(detail);
            }

            // Delete the stock
            _repo.StockRepo.Delete(stock);

            StockDTO stockDTO = _mapper.Map<StockDTO>(stock);

            await _repo.CompleteAsync();

            return new GenericResponseDto<StockDTO>
            {
                Data = stockDTO,
                Message = "Stock deleted successfully",
                Status = 200
            };
        }

        // Get Stock By Stock ID
        public async Task<GenericResponseDto<StockDTO>> GetStockByIdAsync(int id)
        {
            // Find stock by ID with all related entities
            var stock = await _repo.StockRepo.FindAsync(
                s => s.Id == id,
                new[] {
                    "StockDetails",
                    "StockDetails.Feature",
                    "StockDetails.FeatureValue",
                    "Product"
                }
            );

            // Check if stock exists
            if (stock == null)
            {
                _logger.LogError($"Stock with ID {id} not found.");
                throw new NotFoundException($"Stock with ID {id} not found.");
            }

            // Map stock entity to DTO
            var stockDto = _mapper.Map<StockDTO>(stock);

            return new GenericResponseDto<StockDTO>
            {
                Data = stockDto,
                Message = "Stock retrieved successfully",
                Status = 200
            };

        }

        // Get All Stocks
        public async Task<GenericResponseDto<List<StockDTO>>> GetAllStocksAsync()
        {
            // Check if the request came from Admin to return all stocks
            var currentUserId = "1a44c91f-138e-4cf2-a5ef-915e5c882673"; //GetCurrentUserId();
            if (currentUserId == "1a44c91f-138e-4cf2-a5ef-915e5c882673")
            {
                // Get all stocks with related entities
                var stocks = await _repo.StockRepo.FindAllAsync(s => s == s, new[] {
                     "StockDetails",
                     "StockDetails.Feature",
                     "StockDetails.FeatureValue",
                     "Product" });

                // Check if any stocks were found
                if (stocks == null || !stocks.Any())
                {
                    _logger.LogError("No stocks found.");
                    return new GenericResponseDto<List<StockDTO>>
                    {
                        Data = new List<StockDTO>(),
                        Message = "No stocks found",
                        Status = 404
                    };
                }

                // Map stock entities to DTOs
                var stockDtos = _mapper.Map<List<StockDTO>>(stocks);

                return new GenericResponseDto<List<StockDTO>>
                {
                    Data = stockDtos,
                    Message = "Stocks retrieved successfully",
                    Status = 200
                };
            }

            // Get stocks for the current user
            var stocksForUser = await _repo.StockRepo.FindAllAsync(
                s => s.Product.SellerId == currentUserId,
                new[] { "StockDetails", "Product" }
            );

            // Check if any stocks were found for the user
            if (stocksForUser == null || !stocksForUser.Any())
            {
                _logger.LogError($"No stocks found for user ID {currentUserId}.");
                return new GenericResponseDto<List<StockDTO>>
                {
                    Data = null,
                    Message = "No stocks found for the current user",
                    Status = 404
                };
            }

            // Map stock entities to DTOs
            var userStockDtos = _mapper.Map<List<StockDTO>>(stocksForUser);
            return new GenericResponseDto<List<StockDTO>>
            {
                Data = userStockDtos,
                Message = "Stocks retrieved successfully for the current user",
                Status = 200
            };
        }



        // Get Stocks By Product ID
        public async Task<GenericResponseDto<List<StockDTO>>> GetStocksByProductIdAsync(int productId)
        {
            // Find stocks by product ID
            var stocks = await _repo.StockRepo.FindAllAsync(s => s.ProductId == productId, ["StockDetails", "Product"]);

            // Check if any stocks were found
            if (stocks == null || !stocks.Any())
            {
                _logger.LogError($"No stocks found for product ID {productId}.");
                return null;
            }

            // Map stock entities to DTOs
            var stockDtos = _mapper.Map<List<StockDTO>>(stocks);

            return new GenericResponseDto<List<StockDTO>>
            {
                Data = stockDtos,
                Message = "Stocks retrieved successfully",
                Status = 200
            };
        }


        // helper function
        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return userId;
        }
    }
}
