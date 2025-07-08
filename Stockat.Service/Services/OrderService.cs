using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.DTOs.StockDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class OrderService : IOrderService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    // Add Order
    public async Task<GenericResponseDto<AddOrderDTO>> AddOrderAsync(AddOrderDTO orderDto)
    {
        try
        {
            // Map DTO to entity
            var orderEntity = _mapper.Map<OrderProduct>(orderDto);
            // Getting Order quantity from stock quantity incase the order quantity is not provided or wrong
            var stock = await _repo.StockRepo.GetByIdAsync(orderDto.StockId);
            if (stock == null)
            {
                _logger.LogError("Stock not found for the given StockId.");
                return new GenericResponseDto<AddOrderDTO>
                {
                    Status = 404,
                    Message = "Stock not found."
                };
            }
            // Asigning the stock quantity to the order quantity
            orderEntity.Quantity = stock.Quantity;
            // Add the order to the repository
            await _repo.OrderRepo.AddAsync(orderEntity);
            // Set the Stock Status to SoldOut
            stock.StockStatus = StockStatus.SoldOut;
            // Update the stock in the repository
            _repo.StockRepo.Update(stock);

            await _repo.CompleteAsync();
            // Map back to DTO for response
            var responseDto = _mapper.Map<AddOrderDTO>(orderEntity);
            return new GenericResponseDto<AddOrderDTO>
            {
                Status = 201,
                Data = responseDto,
                Message = "Order added successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding order: {ex.Message}");
            return new GenericResponseDto<AddOrderDTO>
            {
                Status = 500,
                Message = "An error occurred while adding the order."
            };
        }
    }

    // Add Request
    public async Task<GenericResponseDto<AddRequestDTO>> AddRequestAsync(AddRequestDTO requestDto)
    {
        await _repo.BeginTransactionAsync();
        try
        {
            // 1. Create and add the stock first
            var stockEntity = new Stock
            {
                ProductId = requestDto.ProductId,
                Quantity = requestDto.Quantity,
                StockStatus = StockStatus.SoldOut,
                StockDetails = requestDto.Stock.StockDetails.Select(sd => new StockDetails
                {
                    FeatureId = sd.FeatureId,
                    FeatureValueId = sd.FeatureValueId
                }).ToList()
            };

            await _repo.StockRepo.AddAsync(stockEntity);
            await _repo.CompleteAsync(); // Save to get the stock ID

            // Get the Buyer ID from the HTTP context
            // var buyerId = GetCurrentUserId();
            // if (string.IsNullOrEmpty(buyerId))
            // {
            //     _logger.LogError("Buyer ID not found in the HTTP context.");
            //     return new GenericResponseDto<AddRequestDTO>
            //     {
            //         Status = 400,
            //         Message = "Buyer ID is required."
            //     };
            // }
            // requestDto.BuyerId = buyerId;

            

            // 2. Create and add the order
            var orderEntity = new OrderProduct
            {
                ProductId = requestDto.ProductId,
                Quantity = requestDto.Quantity,
                Price = requestDto.Price,
                OrderType = OrderType.Request,
                Status = OrderStatus.PendingSeller,
                StockId = stockEntity.Id,
                SellerId = requestDto.SellerId,
                BuyerId = requestDto.BuyerId,
                PaymentId = requestDto.PaymentId,
                PaymentStatus = requestDto.PaymentStatus,
                Description = requestDto.Description,
                CraetedAt = DateTime.UtcNow
            };

            await _repo.OrderRepo.AddAsync(orderEntity);
            await _repo.CompleteAsync();
            await _repo.CommitTransactionAsync();

            // 3. Map back to DTO for response
            var responseDto = new AddRequestDTO
            {
                ProductId = orderEntity.ProductId,
                Quantity = orderEntity.Quantity,
                Price = orderEntity.Price,
                OrderType = orderEntity.OrderType,
                Status = orderEntity.Status,
                StockId = orderEntity.StockId,
                SellerId = orderEntity.SellerId,
                BuyerId = orderEntity.BuyerId,
                PaymentId = orderEntity.PaymentId,
                PaymentStatus = orderEntity.PaymentStatus,
                Description = orderEntity.Description,
                Stock = _mapper.Map<AddStockDTO>(stockEntity)
            };

            return new GenericResponseDto<AddRequestDTO>
            {
                Status = 201,
                Data = responseDto,
                Message = "Request created successfully with associated stock."
            };
        }
        catch (Exception ex)
        {
            await _repo.RollbackTransactionAsync();
            _logger.LogError($"Error creating request with stock: {ex.Message}");
            return new GenericResponseDto<AddRequestDTO>
            {
                Status = 500,
                Message = $"An error occurred while creating the request: {ex.Message}"
            };
        }
    }



    // Update Order Status By its owner(Seller)
    public async Task<GenericResponseDto<OrderDTO>> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        // Open Transaction
        await _repo.BeginTransactionAsync(); // Open transaction

        try
        {
            // Fetch the order by ID
            var order = await _repo.OrderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogError($"Order with ID {orderId} not found.");
                return new GenericResponseDto<OrderDTO>
                {
                    Status = 404,
                    Message = "Order not found."
                };
            }
            // Update the order status
            if (status == OrderStatus.Pending)
            {
                _logger.LogError("Cannot update order status to Pending.");
                return new GenericResponseDto<OrderDTO>
                {
                    Status = 400,
                    Message = "Order status cannot be updated to Pending."
                };
            }
            // if the status is cancelled, we need to update the stock status to ForSale
            if (status == OrderStatus.Cancelled)
            {
                var stock = await _repo.StockRepo.GetByIdAsync(order.StockId);
                if (stock == null)
                {
                    _logger.LogError("Stock not found for the given StockId.");
                    return new GenericResponseDto<OrderDTO>
                    {
                        Status = 404,
                        Message = "Stock not found."
                    };
                }
                // Update the stock status to ForSale
                stock.StockStatus = StockStatus.ForSale;
                _repo.StockRepo.Update(stock);
                order.Status = status;
                _repo.OrderRepo.Update(order);
            }
            else
            {
                order.Status = status;
                _repo.OrderRepo.Update(order);
            }
            await _repo.CompleteAsync();
            await _repo.CommitTransactionAsync(); // Commit the transaction

            // Map to DTO for response

            var orderDto = _mapper.Map<OrderDTO>(order);

            return new GenericResponseDto<OrderDTO>
            {
                Status = 200,
                Data = orderDto,
                Message = "Order status updated successfully."
            };
        }
        catch (Exception ex)
        {
            await _repo.RollbackTransactionAsync(); // Rollback on error
            _logger.LogError($"Error updating order status: {ex.Message}");
            return new GenericResponseDto<OrderDTO>
            {
                Status = 500,
                Message = "An error occurred while updating the order status."
            };
        }
    }



    // Get All Orders For Seller
    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerOrdersAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            //var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("User ID not found in the HTTP context.");
            //    return new GenericResponseDto<IEnumerable<OrderDTO>>
            //    {
            //        Status = 400,
            //        Message = "User ID is required."
            //    };
            //}

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == o.SellerId && o.OrderType == OrderType.Order, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found."
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = "Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving orders."
            };
        }

    }
    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllSellerRequestOrdersAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            //var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("User ID not found in the HTTP context.");
            //    return new GenericResponseDto<IEnumerable<OrderDTO>>
            //    {
            //        Status = 400,
            //        Message = "User ID is required."
            //    };
            //}

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == o.SellerId && o.OrderType == OrderType.Request, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No Request orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found."
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = " Request Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller Request orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving Request orders."
            };
        }

    }

    // Get All Orders For Buyers
    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllBuyerOrdersAsync()
    {
        try
        {
            //Get the user ID from the HTTP context
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID not found in the HTTP context.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 400,
                    Message = "User ID is required."
                };
            }

            // Fetch orders for the Buyers
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.BuyerId == userId && o.OrderType == OrderType.Order, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found.",
                    Data = new List<OrderDTO>()
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = "Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving orders."
            };
        }

    }
    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllBuyerRequestOrdersAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID not found in the HTTP context.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 400,
                    Message = "User ID is required."
                };
            }

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.BuyerId == userId && o.OrderType == OrderType.Request, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No Request orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found.",
                    Data = new List<OrderDTO>()

                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = " Request Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller Request orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving Request orders."
            };
        }

    }






    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllOrdersandRequestforAdminAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            //var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("User ID not found in the HTTP context.");
            //    return new GenericResponseDto<IEnumerable<OrderDTO>>
            //    {
            //        Status = 400,
            //        Message = "User ID is required."
            //    };
            //}

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.Id != null, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No Request orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found."
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = " Request Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller Request orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving Request orders."
            };
        }
    }


    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllUserOrdersAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            //var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("User ID not found in the HTTP context.");
            //    return new GenericResponseDto<IEnumerable<OrderDTO>>
            //    {
            //        Status = 400,
            //        Message = "User ID is required."
            //    };
            //}

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == o.SellerId && o.OrderType == OrderType.Order, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found."
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = "Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving orders."
            };
        }

    }
    public async Task<GenericResponseDto<IEnumerable<OrderDTO>>> GetAllUserRequestOrdersAsync()
    {
        try
        {
            // Get the user ID from the HTTP context
            //var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            //if (string.IsNullOrEmpty(userId))
            //{
            //    _logger.LogError("User ID not found in the HTTP context.");
            //    return new GenericResponseDto<IEnumerable<OrderDTO>>
            //    {
            //        Status = 400,
            //        Message = "User ID is required."
            //    };
            //}

            // Fetch orders for the seller
            //var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == userId,[]);

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.SellerId == o.SellerId && o.OrderType == OrderType.Request, ["Seller", "Buyer"]);
            if (orders == null || !orders.Any())
            {
                _logger.LogInfo("No Request orders found for the seller.");
                return new GenericResponseDto<IEnumerable<OrderDTO>>
                {
                    Status = 404,
                    Message = "No orders found."
                };
            }
            // Map to DTOs
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 200,
                Data = orderDtos,
                Message = " Request Orders retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving seller Request orders: {ex.Message}");
            return new GenericResponseDto<IEnumerable<OrderDTO>>
            {
                Status = 500,
                Message = "An error occurred while retrieving Request orders."
            };
        }

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
