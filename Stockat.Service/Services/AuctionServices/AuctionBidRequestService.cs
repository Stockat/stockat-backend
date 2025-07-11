using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices.IAuctionServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services.AuctionServices
{
    // AuctionBidRequestService.cs
    public class AuctionBidRequestService : IAuctionBidRequestService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public AuctionBidRequestService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuctionBidRequestDto>> GetBidsByUserIdAsync(string userId)
        {
            await _repositoryManager.BeginTransactionAsync();

            try
            {
                var bids = await _repositoryManager.AuctionBidRequestRepo
                    .FindAllAsync(b => b.BidderId == userId, new[] { "Auction" });

                await _repositoryManager.CommitTransactionAsync();

                return _mapper.Map<IEnumerable<AuctionBidRequestDto>>(bids);
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw; // or return empty/fallback result
            }
        }


        public async Task<AuctionBidRequestDto> CreateBidAsync(AuctionBidRequestCreateDto dto)
        {
            await _repositoryManager.BeginTransactionAsync();

            try
            {
                var auction = await _repositoryManager.AuctionRepo.GetByIdAsync(dto.AuctionId);

                if (auction == null) throw new NotFoundException("Auction not found");

                var now = DateTime.UtcNow;

                //chexk if not closed
                if (auction.IsClosed || auction.StartTime > now || auction.EndTime <= now)
                    throw new BusinessException("Auction is not open for bidding");

                
                if (auction.SellerId == dto.BidderId) throw new BusinessException("Seller cannot place a bid on their own auction");

                //check bid amount validation
                var minimumValidBid = auction.CurrentBid + auction.IncrementUnit;

                if (dto.BidAmount < minimumValidBid)
                    throw new BusinessException($"Bid must be at least {minimumValidBid}");

                //check for duplication --> maybe removed
                //var existingBid = await _repositoryManager.AuctionBidRequestRepo.FindAsync(
                //    b => b.AuctionId == dto.AuctionId && b.BidderId == dto.BidderId);

                //if (existingBid != null)
                //    throw new BusinessException("You already placed a bid on this auction");
               
                var bid = _mapper.Map<AuctionBidRequest>(dto);
                await _repositoryManager.AuctionBidRequestRepo.AddAsync(bid);

                auction.CurrentBid = dto.BidAmount;
                auction.BuyerId = dto.BidderId;

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();

                return _mapper.Map<AuctionBidRequestDto>(bid);
            }
            catch
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task<AuctionBidRequestDto> GetBidByIdAsync(int id)
        {
            var bid = await _repositoryManager.AuctionBidRequestRepo.GetByIdAsync(id);

            if (bid == null)
                throw new NotFoundException($"Bid with ID {id} was not found.");

            return _mapper.Map<AuctionBidRequestDto>(bid);
        }


        public async Task<IEnumerable<AuctionBidRequestDto>> GetBidsByAuctionAsync(int auctionId)
        {
            var auction = await _repositoryManager.AuctionRepo.GetByIdAsync(auctionId);

            if (auction == null)
                throw new NotFoundException($"Auction with ID {auctionId} was not found.");

            var bids = await _repositoryManager.AuctionBidRequestRepo.FindAllAsync(b => b.AuctionId == auctionId, includes: new[] { "BidderUser" });

            if (!bids.Any()) throw new NotFoundException($"No bids found for auction ID {auctionId}.");

            return _mapper.Map<IEnumerable<AuctionBidRequestDto>>(bids);
        }


        public async Task DeleteBidAsync(int id)
        {
            var bid = await _repositoryManager.AuctionBidRequestRepo.GetByIdAsync(id);

            if (bid == null)
                throw new NotFoundException($"Bid with ID {id} was not found and cannot be deleted.");

            var auction = await _repositoryManager.AuctionRepo.GetByIdAsync(bid.AuctionId);

            if (auction == null)
                throw new NotFoundException($"Related auction with ID {bid.AuctionId} not found.");


            //prrevent deletion if active auction
            if (auction.IsClosed)
                throw new BusinessException("Cannot delete bid from a closed auction.");

            await _repositoryManager.BeginTransactionAsync();
            try
            {
                _repositoryManager.AuctionBidRequestRepo.Delete(bid);

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
