using AutoMapper;
using CloudinaryDotNet;
using Microsoft.Extensions.Logging;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Stockat.Core.IServices.IAuctionServices;
using Stripe;
using Stripe.Checkout;
//using Stripe.BillingPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services.AuctionServices
{
    public class AuctionOrderService : IAuctionOrderService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly IServiceManager _serviceManager;
        private readonly ILoggerManager _logger;

        public AuctionOrderService(IRepositoryManager repositoryManager, IMapper mapper, IServiceManager serviceManager, ILoggerManager logger)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _serviceManager = serviceManager;
            _logger = logger;
        }

        public async Task<AuctionOrderDto> CreateOrderForWinningBidAsync(int auctionId)
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {
                var auction = await _repositoryManager.AuctionRepo.FindAsync(
                    a => a.Id == auctionId,
                    includes: new[] { "AuctionBidRequest" });

                if (auction == null)
                    throw new ArgumentException("Auction not found");

                if (!auction.IsClosed)
                    throw new InvalidOperationException("Auction is still active");

                if (auction.BuyerId == null)
                    throw new InvalidOperationException("Auction has no winner");

                //get winning bid
                var winningBid = auction.AuctionBidRequest
                    .Where(b => b.BidderId == auction.BuyerId)
                    .OrderByDescending(b => b.BidAmount)
                    .FirstOrDefault();

                if (winningBid == null)
                    throw new InvalidOperationException("Winning bid not found");

                var existingOrder = await _repositoryManager.AuctionOrderRepo.FindAsync(o => o.AuctionId == auctionId);

                if (existingOrder != null)
                    throw new InvalidOperationException("Order already exists for this auction");


                var newOrder = new AuctionOrder
                {
                    AuctionId = auctionId,
                    AuctionRequestId = winningBid.Id,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending
                };

                await _repositoryManager.AuctionOrderRepo.AddAsync(newOrder);

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();

                return _mapper.Map<AuctionOrderDto>(newOrder);
            }
            catch
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task MarkPaymentFailedAsync(int orderId, string reason = null)
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {
                var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {orderId} not found.");
                order.Status = OrderStatus.PaymentFailed;
                order.PaymentStatus = PaymentStatus.Failed;
                order.Notes = reason;
                _repositoryManager.AuctionOrderRepo.Update(order);
                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw new InvalidOperationException("Failed to mark payment as failed.", ex);
            }
        }

        public async Task UpdateOrderAddressInfoAsync(int orderId, string shippingAddress, string recipientName, string phoneNumber, string notes)
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {
                var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {orderId} not found.");
                order.ShippingAddress = shippingAddress;
                order.RecipientName = recipientName;
                order.PhoneNumber = phoneNumber;
                order.Notes = notes;
                _repositoryManager.AuctionOrderRepo.Update(order);
                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw new InvalidOperationException("Failed to update order address info.", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {
                var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {orderId} not found.");

                // Enforce allowed forward transitions only
               // var currentStatus = order.Status;
               // if (!IsValidStatusTransition(currentStatus, newStatus))
                //    throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}.");

                order.Status = newStatus;
                _repositoryManager.AuctionOrderRepo.Update(order);

                // If cancelled, restore stock status
                if (newStatus == OrderStatus.Cancelled)
                {
                    var auction = await _repositoryManager.AuctionRepo.GetByIdAsync(order.AuctionId);
                    if (auction != null)
                    {
                        var stock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId);
                        if (stock != null)
                        {
                            stock.StockStatus = Stockat.Core.Enums.StockStatus.ForSale;
                            _repositoryManager.StockRepo.Update(stock);
                        }
                    }
                }

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw new InvalidOperationException("Failed to update order status.", ex);
            }
        }

        // Helper: Only allow forward transitions per role
        private bool IsValidStatusTransition(OrderStatus? current, OrderStatus next)
        {
            // Define allowed transitions (forward only)
            var transitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                // Buyer
                { OrderStatus.PendingBuyer, new List<OrderStatus> { OrderStatus.Payed, OrderStatus.Cancelled } },
                { OrderStatus.Payed, new List<OrderStatus> { OrderStatus.Processing } },
                { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
                { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Completed } },
                // Seller
                { OrderStatus.PendingSeller, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
                { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Ready } },
                { OrderStatus.Ready, new List<OrderStatus> { OrderStatus.Shipped } },
                // Admin (can do all forward transitions)
                { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Ready, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.PaymentFailed } },
                { OrderStatus.PaymentFailed, new List<OrderStatus> { OrderStatus.Cancelled } },
            };
            if (current == null) return false;
            if (transitions.TryGetValue(current.Value, out var allowed))
            {
                return allowed.Contains(next);
            }
            // Allow admin to set to Cancelled, PaymentFailed, Completed at any stage (forward only)
            if (next == OrderStatus.Cancelled || next == OrderStatus.PaymentFailed || next == OrderStatus.Completed)
                return true;
            return false;
        }

        public async Task<IEnumerable<AuctionOrderDto>> GetAllOrdersAsync()
        {
            var orders = await _repositoryManager.AuctionOrderRepo.FindAllAsync(
                    criteria: x => true,
                    includes: new[] { "Auction", "AuctionRequest", "Auction.SellerUser", "AuctionRequest.BidderUser" }
                );

            var orderDtos = _mapper.Map<IEnumerable<AuctionOrderDto>>(orders);
            
            // Populate additional information
            foreach (var orderDto in orderDtos)
            {
                var order = orders.FirstOrDefault(o => o.Id == orderDto.Id);
                if (order != null)
                {
                    // Populate seller information
                    if (order.Auction?.SellerUser != null)
                    {
                        orderDto.SellerId = order.Auction.SellerId;
                        orderDto.SellerName = $"{order.Auction.SellerUser.FirstName} {order.Auction.SellerUser.LastName}";
                    }
                    
                    // Populate buyer information
                    if (order.AuctionRequest?.BidderUser != null)
                    {
                        orderDto.BuyerId = order.AuctionRequest.BidderId;
                        orderDto.BuyerName = $"{order.AuctionRequest.BidderUser.FirstName} {order.AuctionRequest.BidderUser.LastName}";
                    }
                    
                    // Populate auction information
                    if (order.Auction != null)
                    {
                        orderDto.AuctionTitle = order.Auction.Name;
                        orderDto.AuctionDescription = order.Auction.Description;
                    }
                    
                    // Populate winning bid amount
                    if (order.AuctionRequest != null)
                    {
                        orderDto.WinningBidAmount = order.AuctionRequest.BidAmount;
                        orderDto.AmountPaid = order.AuctionRequest.BidAmount;
                    }
                }
            }
            
            return orderDtos;
        }


        public async Task<AuctionOrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _repositoryManager.AuctionOrderRepo.FindAsync(o => o.Id == id,
                includes: new[] { "Auction", "AuctionRequest", "Auction.SellerUser", "AuctionRequest.BidderUser" });

            if (order == null)
                throw new NullReferenceException();

            var orderDto = _mapper.Map<AuctionOrderDto>(order);
            
            // Populate additional information
            if (order.Auction?.SellerUser != null)
            {
                orderDto.SellerId = order.Auction.SellerId;
                orderDto.SellerName = $"{order.Auction.SellerUser.FirstName} {order.Auction.SellerUser.LastName}";
            }
            
            if (order.AuctionRequest?.BidderUser != null)
            {
                orderDto.BuyerId = order.AuctionRequest.BidderId;
                orderDto.BuyerName = $"{order.AuctionRequest.BidderUser.FirstName} {order.AuctionRequest.BidderUser.LastName}";
            }
            
            if (order.Auction != null)
            {
                orderDto.AuctionTitle = order.Auction.Name;
                orderDto.AuctionDescription = order.Auction.Description;
            }
            
            if (order.AuctionRequest != null)
            {
                orderDto.WinningBidAmount = order.AuctionRequest.BidAmount;
                orderDto.AmountPaid = order.AuctionRequest.BidAmount;
            }

            return orderDto;
        }

        public async Task<IEnumerable<AuctionOrderDto>> GetOrdersByUserAsync(string userId)
        {
            var orders = await _repositoryManager.AuctionOrderRepo.FindAllAsync(o => o.Auction.SellerId == userId || o.AuctionRequest.BidderId == userId,
                includes: new[] { "Auction", "AuctionRequest", "Auction.SellerUser", "AuctionRequest.BidderUser" });

            var orderDtos = _mapper.Map<IEnumerable<AuctionOrderDto>>(orders);
            
            // Populate additional information
            foreach (var orderDto in orderDtos)
            {
                var order = orders.FirstOrDefault(o => o.Id == orderDto.Id);
                if (order != null)
                {
                    // Populate seller information
                    if (order.Auction?.SellerUser != null)
                    {
                        orderDto.SellerId = order.Auction.SellerId;
                        orderDto.SellerName = $"{order.Auction.SellerUser.FirstName} {order.Auction.SellerUser.LastName}";
                    }
                    
                    // Populate buyer information
                    if (order.AuctionRequest?.BidderUser != null)
                    {
                        orderDto.BuyerId = order.AuctionRequest.BidderId;
                        orderDto.BuyerName = $"{order.AuctionRequest.BidderUser.FirstName} {order.AuctionRequest.BidderUser.LastName}";
                    }
                    
                    // Populate auction information
                    if (order.Auction != null)
                    {
                        orderDto.AuctionTitle = order.Auction.Name;
                        orderDto.AuctionDescription = order.Auction.Description;
                    }
                    
                    // Populate winning bid amount
                    if (order.AuctionRequest != null)
                    {
                        orderDto.WinningBidAmount = order.AuctionRequest.BidAmount;
                        orderDto.AmountPaid = order.AuctionRequest.BidAmount;
                    }
                }
            }

            return orderDtos;
        }

        public async Task<AuctionOrderDto> GetOrderByAuctionIdAsync(int auctionId)
        {
            var order = await _repositoryManager.AuctionOrderRepo.FindAsync(o => o.AuctionId == auctionId,
                includes: new[] { "Auction", "AuctionRequest", "Auction.SellerUser", "AuctionRequest.BidderUser" });

            if (order == null) return null;

            return _mapper.Map<AuctionOrderDto>(order);
        }

        public async Task ProcessPaymentAsync(int orderId, ProcessPaymentDto paymentDto)
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {
                var order = await _repositoryManager.AuctionOrderRepo.FindAsync(o => o.Id == orderId, includes: new[] { "Auction", "AuctionRequest" });

                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Order payment already placed");


                order.PaymentTransactionId = paymentDto.PaymentTransactionId;
                order.PaymentStatus = PaymentStatus.Paid;

                order.Status = paymentDto.PaymentSuccess? OrderStatus.Completed :OrderStatus.PaymentFailed;

                _repositoryManager.AuctionOrderRepo.Update(order);

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch
            {
                await _repositoryManager.RollbackTransactionAsync();

                throw;
            }
        }

        public async Task UpdateOrderPaymentStatus(int orderId, PaymentStatus status)
        {
            var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
            if (order == null) return;

            order.PaymentStatus = status;
            _repositoryManager.AuctionOrderRepo.Update(order);
            await _repositoryManager.CompleteAsync();
        }

        public async Task RevertAuctionStockAsync(int orderId)
        {
            var order = await _repositoryManager.AuctionOrderRepo.FindAsync(
                o => o.Id == orderId,
                new[] { "Auction", "Auction.Stock" }
            );

            if (order?.Auction?.Stock != null)
            {
                order.Auction.Stock.StockStatus = StockStatus.ForSale;
                _repositoryManager.StockRepo.Update(order.Auction.Stock);
                await _repositoryManager.CompleteAsync();
            }
        }

        public async Task UpdateStripePaymentID(int orderId, string sessionId, string paymentIntentId)
        {
            var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
            if (order == null) 
            {
                _logger.LogError($"Order {orderId} not found when updating Stripe payment ID");
                return;
            }

            _logger.LogInfo($"Updating Stripe payment ID for order {orderId}. SessionId: {sessionId}, PaymentIntentId: {paymentIntentId}");

            if (!string.IsNullOrEmpty(sessionId))
            {
                order.StripeSessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                order.StripePaymentIntentId = paymentIntentId;
                Console.WriteLine("payment intent" + paymentIntentId);
                // order.PaymentStatus = PaymentStatus.Paid;
                // _logger.LogInfo($"Order {orderId} payment status updated to Paid");
            }
            order.PaymentStatus = PaymentStatus.Paid;
            order.Status=OrderStatus.Processing;
            Console.WriteLine("update in id paid");
            _repositoryManager.AuctionOrderRepo.Update(order);
            await _repositoryManager.CompleteAsync();
            
            _logger.LogInfo($"Stripe payment ID update completed for order {orderId}");
        }


        public async Task HandleAuctionOrderCompletion(Session session, string orderId)
        {
            int id = int.Parse(orderId);
            _logger.LogInfo($"Handling auction order completion for order {id}");
            
            await UpdateOrderPaymentStatus(id, PaymentStatus.Paid);
            await UpdateOrderStatusAsync(id, OrderStatus.Processing);
            
            _logger.LogInfo($"Auction order {id} payment completed successfully. Status: Processing, PaymentStatus: Paid");

            // Optional: Generate invoice
            //await _invoiceService.GenerateAuctionInvoiceAsync(id);
        }

        //public async Task UpdateOrderPaymentStatus(int orderId, PaymentStatus status)   
        //{
        //    var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
        //    if (order == null) 
        //    {
        //        _logger.LogError($"Order {orderId} not found when updating payment status");
        //        return;
        //    }

        //    order.PaymentStatus = status;
        //    _repositoryManager.AuctionOrderRepo.Update(order);
        //    await _repositoryManager.CompleteAsync();
            
        //    _logger.LogInfo($"Order {orderId} payment status updated to {status}");
        //}

        //public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        //{
        //    var order = await _repositoryManager.AuctionOrderRepo.GetByIdAsync(orderId);
        //    if (order == null) 
        //    {
        //        _logger.LogError($"Order {orderId} not found when updating order status");
        //        return;
        //    }

        //    order.Status = status;
        //    _repositoryManager.AuctionOrderRepo.Update(order);
        //    await _repositoryManager.CompleteAsync();
            
        //    _logger.LogInfo($"Order {orderId} status updated to {status}");
        //}

        public async Task HandleAuctionOrderFailure(Session session, string orderId)
        {
            int id = int.Parse(orderId);
            await UpdateOrderPaymentStatus(id, PaymentStatus.Failed);
            await _serviceManager.AuctionOrderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);

            // Revert stock status
           // await _serviceManager.AuctionOrderService.RevertAuctionStockAsync(id);
        }

        public async Task<GenericResponseDto<AuctionOrderDto>> CreateStripeCheckoutSessionAsync(int orderId, string buyerId)
        {
            try
            {
                if (string.IsNullOrEmpty(buyerId))
                {
                    _logger.LogError($"Unauthorized buyer {buyerId} tried to create an auction order checkout session.");
                    throw new UnauthorizedAccessException("You are not authorized to create a checkout session.");
                }

                var order = await _repositoryManager.AuctionOrderRepo.FindAsync(
                    o => o.Id == orderId && o.AuctionRequest.BidderId == buyerId,
                    includes: new[] { "AuctionRequest", "Auction" }
                );

                if (order == null)
                {
                    _logger.LogError($"Auction order {orderId} not found for buyer {buyerId}.");
                    return new GenericResponseDto<AuctionOrderDto>
                    {
                        Status = 404,
                        Message = "Auction order not found or you are not authorized to pay for this order."
                    };
                }

                // Check if order is ready for payment
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PendingBuyer)
                {
                    _logger.LogError($"Auction order {orderId} is not ready for checkout.");
                    return new GenericResponseDto<AuctionOrderDto>
                    {
                        Status = 400,
                        Message = "Auction order is not ready for checkout. Order must be in pending status."
                    };
                }

                // Check if payment has already been completed
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    _logger.LogError($"Auction order {orderId} has already been paid.");
                    return new GenericResponseDto<AuctionOrderDto>
                    {
                        Status = 400,
                        Message = "Payment has already been completed for this order."
                    };
                }

                // Validate order amount
                decimal amount = order.AuctionRequest.BidAmount;
                if (amount <= 0)
                {
                    _logger.LogError($"Invalid order amount for auction order {orderId}: {amount}");
                    return new GenericResponseDto<AuctionOrderDto>
                    {
                        Status = 400,
                        Message = "Invalid order amount"
                    };
                }

                // Fixed: Added null checks for auction data
                //var auctionName = order.Auction?.Name ?? "Auction Item";
                //var auctionDescription = order.Auction?.Description ?? "Won auction item";

                var auctionName = order.Auction?.Name ?? "Auction Item";

                // Handle empty description specifically
                var auctionDescription = !string.IsNullOrWhiteSpace(order.Auction?.Description)
                    ? order.Auction.Description
                    : "Won auction item";

                // Create Stripe session
                var sessionItems = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = auctionName,
                            Description = auctionDescription
                        }
                    },
                    Quantity = 1,
                };

                var options = new SessionCreateOptions
                {
                    SuccessUrl = "https://stockat-frontend-git-main-stockat-groups-projects.vercel.app/",
                    CancelUrl = "https://stockat-frontend-git-main-stockat-groups-projects.vercel.app/profile",
                    LineItems = new List<SessionLineItemOptions> { sessionItems },
                    Mode = "payment",
                    Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
                { "type", "auction_order" }
            }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                // Update order with session ID
                await UpdateStripePaymentID(order.Id, session.Id, session.PaymentIntentId);

                return new GenericResponseDto<AuctionOrderDto>
                {
                    Status = 201,
                    Data = null, // Don't return the order data for now
                    Message = "Stripe checkout session created successfully.",
                    RedirectUrl = session.Url
                };
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError($"Stripe error creating checkout session for order {orderId}: {stripeEx.Message}");
                return new GenericResponseDto<AuctionOrderDto>
                {
                    Status = 500,
                    Message = $"Payment processing error: {stripeEx.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating checkout session for order {orderId}: {ex.Message}");
                return new GenericResponseDto<AuctionOrderDto>
                {
                    Status = 500,
                    Message = "An unexpected error occurred while creating the checkout session."
                };
            }
        }

    }
}
