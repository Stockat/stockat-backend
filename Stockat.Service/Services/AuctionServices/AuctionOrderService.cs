using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.IServices.IAuctionServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services.AuctionServices
{
    public class AuctionOrderService : IAuctionOrderService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public AuctionOrderService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
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


        public async Task<AuctionOrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _repositoryManager.AuctionOrderRepo.FindAsync(o => o.Id == id,
                includes: new[] { "Auction", "AuctionRequest", "Auction.SellerUser", "AuctionRequest.BidderUser" });

            if (order != null)
                throw new NullReferenceException();

            return order == null ? null : _mapper.Map<AuctionOrderDto>(order);
        }

        public async Task<IEnumerable<AuctionOrderDto>> GetOrdersByUserAsync(string userId)
        {
            var orders = await _repositoryManager.AuctionOrderRepo.FindAllAsync(o => o.Auction.SellerId == userId || o.AuctionRequest.BidderId == userId,
                includes: new[] { "Auction", "AuctionRequest" });

            return _mapper.Map<IEnumerable<AuctionOrderDto>>(orders);
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
                order.PaymentStatus = paymentDto.PaymentSuccess;

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
    }
}
