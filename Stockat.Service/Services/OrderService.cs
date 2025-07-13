using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;
using Stockat.Core.DTOs.StockDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stripe.Checkout;
using Stripe.Climate;
using Stockat.Core.Consts;
using Stripe;
using CloudinaryDotNet.Actions;
using Stockat.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Stockat.Service.Services;

public class OrderService : IOrderService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly DomainConfigs _domainConfigs;

    public OrderService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo,
        IHttpContextAccessor httpContextAccessor, IEmailService emailService, DomainConfigs domainConfigs)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _domainConfigs = domainConfigs;
    }

    // Add Order
    public async Task<GenericResponseDto<AddOrderDTO>> AddOrderAsync(AddOrderDTO orderDto, string domain)
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
            //*******************************************************************************************************************
            // Begin Stripe 
            var sessionItems = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(orderEntity.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = string.IsNullOrEmpty(orderDto.ProductName) ? "FixedName" : orderDto.ProductName,
                    }
                },
                Quantity = orderEntity.Quantity,
            };
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = _domainConfigs.FrontURL,
                CancelUrl = $"{_domainConfigs.FrontURL}product-stocks/{orderEntity.ProductId}?session_id={{CHECKOUT_SESSION_ID}}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Metadata = new Dictionary<string, string>

    {
        { "orderId", orderEntity.Id.ToString() },
        { "type", "order" }
    }
            };
            options.LineItems.Add(sessionItems);

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);


            // Append sessionId in order
            await UpdateStripePaymentID(orderEntity.Id, session.Id, session.PaymentIntentId);
            // Map back to DTO for response
            var responseDto = _mapper.Map<AddOrderDTO>(orderEntity);

            return new GenericResponseDto<AddOrderDTO>
            {
                Status = 201,
                Data = responseDto,
                Message = "Order added successfully.",
                RedirectUrl = session.Url
            };


            // End Stripe 
            //******************************************************************************************************************

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

    public async Task<GenericResponseDto<UpdateRequestDTO>> AddStripeWithRequestAsync(UpdateRequestDTO requestDto)
    {

        var res = _repo.OrderRepo.GetByIdAsync(requestDto.Id).Result;

        if (res == null)
            return new GenericResponseDto<UpdateRequestDTO>()
            {
                Data = requestDto,
                Status = 404,
                Message = "Could not Found an Req Order with id =" + requestDto.Id
            };

        //*******************************************************************************************************************
        // Begin Stripe 
        var sessionItems = new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(requestDto.Price * 100),
                Currency = "usd",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = requestDto.ProductName,
                }
            },
            Quantity = requestDto.Quantity,
        };
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            SuccessUrl = $"{_domainConfigs.FrontURL}profile",
            CancelUrl = $"{_domainConfigs.FrontURL}profile",
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
            Mode = "payment",
            Metadata = new Dictionary<string, string>
    {
        { "orderId", requestDto.Id.ToString() },
        { "type", "req" }
    }
        };
        options.LineItems.Add(sessionItems);

        var service = new Stripe.Checkout.SessionService();
        Stripe.Checkout.Session session = service.Create(options);


        // Append sessionId in order
        await UpdateStripePaymentID(requestDto.Id, session.Id, session.PaymentIntentId);

        return new GenericResponseDto<UpdateRequestDTO>
        {
            Status = 201,
            Data = requestDto,
            Message = "Order Request Updated successfully.",
            RedirectUrl = session.Url
        };

    }


    // Stripe Internals 
    public async Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
    {

        var order = await _repo.OrderRepo.GetByIdAsync(id);
        if (!string.IsNullOrEmpty(sessionId))
        {
            order.SessionId = sessionId;
        }
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            order.PaymentId = paymentIntentId;
            order.PaymentDate = DateTime.Now;
        }

        _repo.OrderRepo.Update(order);
        await _repo.CompleteAsync();
    }

    public async Task UpdateStatus(int id, OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        var order = await _repo.OrderRepo.GetByIdAsync(id);
        if (order != null)
        {
            order.PaymentStatus = paymentStatus;
            order.Status = orderStatus;
        }
        _repo.OrderRepo.Update(order);
        await _repo.CompleteAsync();
    }


    // Get Order By Id Async
    public async Task<OrderProduct> getOrderByIdAsync(int id)
    {

        return await _repo.OrderRepo.GetByIdAsync(id);
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
                PaymentStatus = PaymentStatus.Pending,
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
                PaymentStatus = orderEntity.PaymentStatus.ToString(),
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
                // Update the stock status to ForSale is the order tyoe is Order
                if (order.OrderType == OrderType.Order)
                {
                    stock.StockStatus = StockStatus.ForSale;
                    _repo.StockRepo.Update(stock);
                }
                // Update the stock status to ForRequest is the order type is Request
                else if (order.OrderType == OrderType.Request)
                {
                    stock.StockStatus = StockStatus.ForRequest;
                    _repo.StockRepo.Update(stock);
                }
                order.Status = status;
                _repo.OrderRepo.Update(order);
            }
            // if the status is processing, we need to update the stock status to SoldOut
            else if (status == OrderStatus.Processing)
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
                stock.StockStatus = StockStatus.SoldOut;
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

    public async Task<GenericResponseDto<OrderDTO>> UpdateRequestOrderStatusAsync(UpdateReqDto updateReq)
    {

        try
        {
            // Fetch the order by ID
            var order = await _repo.OrderRepo.GetByIdAsync(updateReq.Id);
            if (order == null)
            {
                _logger.LogError($"Order with ID {updateReq.Id} not found.");
                return new GenericResponseDto<OrderDTO>
                {
                    Status = 404,
                    Message = "Order not found."
                };
            }

            order.Status = OrderStatus.PendingBuyer;
            order.Price = updateReq.Price;
            order.EstimatedDeliveryTime = updateReq.DeliveryDate;

            _repo.OrderRepo.Update(order);
            await _repo.CompleteAsync();

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
            _logger.LogError($"Error updating Req order status: {ex.Message}");
            return new GenericResponseDto<OrderDTO>
            {
                Status = 500,
                Message = "An error occurred while updating the Req order status."
            };
        }

    }

    // Cancel Order On Payment Faliure
    public async Task<GenericResponseDto<OrderDTO>> CancelOrderOnPaymentFailureAsync(string sessionId)
    {
        // Open Transaction
        await _repo.BeginTransactionAsync(); // Open transaction
        try
        {
            // Fetch the order by session ID
            var order = await _repo.OrderRepo.FindAsync(o => o.SessionId == sessionId, ["Seller", "Buyer", "Product"]);
            if (order == null)
            {
                _logger.LogError($"Order with Session ID {sessionId} not found.");
                return new GenericResponseDto<OrderDTO>
                {
                    Status = 404,
                    Message = "Order not found."
                };
            }

            // Update the order status to Cancelled
            order.Status = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Failed;
            _repo.OrderRepo.Update(order);
            // Update the stock status to ForSale
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
            stock.StockStatus = StockStatus.ForSale;
            _repo.StockRepo.Update(stock);
            await _repo.CompleteAsync();
            await _repo.CommitTransactionAsync(); // Commit the transaction
            // Map to DTO for response
            var orderDto = _mapper.Map<OrderDTO>(order);
            return new GenericResponseDto<OrderDTO>
            {
                Status = 200,
                Data = orderDto,
                Message = "Order cancelled successfully due to payment failure."
            };
        }
        catch (Exception ex)
        {
            await _repo.RollbackTransactionAsync(); // Rollback on error
            _logger.LogError($"Error cancelling order on payment failure: {ex.Message}");
            return new GenericResponseDto<OrderDTO>
            {
                Status = 500,
                Message = "An error occurred while cancelling the order."
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

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.BuyerId == userId && o.OrderType == OrderType.Order, ["Seller", "Buyer", "Product"]);
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

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.BuyerId == userId && o.OrderType == OrderType.Request, ["Seller", "Buyer", "Product"]);
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

            var orders = await _repo.OrderRepo.FindAllAsync(o => o.Id != null, ["Seller", "Buyer", "Product"]);
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

    // Analysis 
    public async Task<GenericResponseDto<Dictionary<OrderType, decimal>>> GetTotalSalesByOrderTypeAsync()
    {
        var res = await _repo.OrderRepo.GetTotalSalesByOrderTypeAsync();

        return new GenericResponseDto<Dictionary<OrderType, decimal>>()
        {

            Data = res,
            Status = 200,
            Message = "Data Fetched Successfully"
        };

    }

    public async Task<GenericResponseDto<Dictionary<OrderType, int>>> GetOrderCountsByTypeAsync()
    {
        var res = await _repo.OrderRepo.GetOrderCountsByTypeAsync();

        return new GenericResponseDto<Dictionary<OrderType, int>>()
        {

            Data = res,
            Status = 200,
            Message = "Data Fetched Successfully"
        };
    }

    public GenericResponseDto<ReportDto> CalculateMonthlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {

        var res = _repo.OrderRepo.CalculateMonthlyRevenueOrderVsStatus(type, status, metricType);

        return new GenericResponseDto<ReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "CalculateMonthlyRevenueOrderVsStatus Fetched Successfully"
        };
    }
    public GenericResponseDto<ReportDto> CalculateWeeklyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        var res = _repo.OrderRepo.CalculateWeeklyRevenueOrderVsStatus(type, status, metricType);

        return new GenericResponseDto<ReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "CalculateWeeklyRevenueOrderVsStatus Fetched Successfully"
        };
    }

    public GenericResponseDto<ReportDto> CalculateYearlyRevenueOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        var res = _repo.OrderRepo.CalculateYearlyRevenueOrderVsStatus(type, status, metricType);

        return new GenericResponseDto<ReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "CalculateYearlyRevenueOrderVsStatus Fetched Successfully"
        };
    }

    public async Task<GenericResponseDto<ReportDto>> CalculateOrdersVsPaymentStatusAsync(OrderType? type, ReportMetricType metricType)
    {
        var res = await _repo.OrderRepo.CalculateOrdersVsPaymentStatusAsync(type, metricType);

        return new GenericResponseDto<ReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "CalculateOrdersVsPaymentStatusAsync Fetched Successfully"
        };
    }






    public GenericResponseDto<TopProductReportDto> GetTopProductPerYearAsync(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        var res = _repo.OrderRepo.GetTopProductPerYearAsync(type, status, metricType);

        return new GenericResponseDto<TopProductReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "GetTopProductPerYearAsync Fetched Successfully"
        };
    }
    public GenericResponseDto<TopProductReportDto> GetTopProductPerMonthAsync(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        var res = _repo.OrderRepo.GetTopProductPerMonthAsync(type, status, metricType);

        return new GenericResponseDto<TopProductReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "GetTopProductPerMonthAsync Fetched Successfully"
        };
    }
    public GenericResponseDto<TopProductReportDto> GetTopProductPerWeekAsync(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        var res = _repo.OrderRepo.GetTopProductPerWeekAsync(type, status, metricType);

        return new GenericResponseDto<TopProductReportDto>()
        {

            Data = res,
            Status = 200,
            Message = "GetTopProductPerWeekAsync Fetched Successfully"
        };
    }

    // Invoice Generator 
    public async Task InvoiceGeneratorAsync(int orderid)
    {

        var order = await _repo.OrderRepo.FindAsync(o => o.Id == orderid, ["Product"]);
        var user = await _repo.UserRepo.GetByIdAsync(order.BuyerId);
        var invoice = InvoiceGenerator.CreateInvoice(order.PaymentId, order.PaymentDate.ToString(), "Credit Card", order.Product.Name,
        order.Quantity, order.Price, "stockatgroup@gmail.com");

        await _emailService.SendEmailAsync(user.Email, "Payment Receipt", invoice);

    }

    public async Task PaymentCancellation()
    {

        var orders = await _repo.OrderRepo.FindAllAsync(o =>
        o.OrderType == OrderType.Order && o.PaymentStatus == PaymentStatus.Pending &&
        o.SessionId != null &&
        o.CraetedAt < DateTime.UtcNow.AddMinutes(-30),
        ["Stock"]);
        if (orders.Any())
        {
            foreach (var order in orders)
            {
                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Failed;
                if (order.Stock != null)
                {
                    order.Stock.StockStatus = StockStatus.ForSale;
                }
            }
        }
        _logger.LogWarn("Cancel Payment Works ");
        await _repo.CompleteAsync();
    }


    public async Task<GenericResponseDto<Dictionary<string, int>>> OrderSummaryCalc()
    {

        var res = await _repo.OrderRepo.GetOrderStatusCountsAsync();

        return new GenericResponseDto<Dictionary<string, int>>()
        {

            Data = res,
            Message = "Order Summary Fetched ",
            Status = 200
        };

    }



    public async Task<OrderProduct> getorderbySessionOrPaymentId(string id)
    {
        try
        {
            var res = await _repo.OrderRepo.FindAsync(o => o.SessionId == id || o.PaymentId == id);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("////////////////////////////////////");
            _logger.LogError("Order Is Not Found");

            return null;

        }
    }



    public async Task<GenericResponseDto<OrderDTO>> UpdateOrderDriverAsync(int orderId, string driverId)
    {
        var order = await _repo.OrderRepo.GetByIdAsync(orderId);
        if (order == null)
        {
            return new GenericResponseDto<OrderDTO>
            {
                Status = 404,
                Message = "Order not found."
            };
        }
        var driver = await _repo.DriverRepo.FindAsync(d => d.Id == driverId);
        if (driver == null)
        {
            return new GenericResponseDto<OrderDTO>
            {
                Status = 404,
                Message = "Driver not found."
            };
        }
        order.DriverId = driverId;
        _repo.OrderRepo.Update(order);
        await _repo.CompleteAsync();
        var dto = _mapper.Map<OrderDTO>(order);
        return new GenericResponseDto<OrderDTO>
        {
            Status = 200,
            Data = dto,
            Message = "Order driver updated successfully."
        };
    }
}
