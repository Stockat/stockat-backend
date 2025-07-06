using AutoMapper;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Stockat.Core;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IRepositories;
using Stockat.Core.IServices;
using Stockat.Core.IServices.IAuctionServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services.AuctionServices
{
    public class AuctionService : IAuctionService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;


        public AuctionService(IMapper mapper,
            ILoggerManager logger,
            IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuctionDetailsDto> AddAuctionAsync(Auction auction)
        {
            if (auction == null)
                throw new NullObjectParameterException(nameof(auction));

            try
            {
                await _repositoryManager.BeginTransactionAsync();

                ValidateAuction(auction);

                var stock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId); //Edit: or find all, with includes, what are includes??
                if (stock == null) throw new NotFoundException("Stock not found");

                //check if quantity suffecient
                if (stock.Quantity < auction.Quantity)
                    throw new BusinessException("Insufficient stock");

                //Edit:check if stock belongs to that sellerid
                stock.Quantity -= auction.Quantity;

                _repositoryManager.StockRepo.Update(stock);

                auction.CurrentBid = auction.StartingPrice;

                await _repositoryManager.AuctionRepo.AddAsync(auction);

                await _repositoryManager.CompleteAsync();

                await _repositoryManager.CommitTransactionAsync();

                return _mapper.Map<AuctionDetailsDto>(auction);
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AuctionDetailsDto> EditAuctionAsync(int id, AuctionUpdateDto auction)
        {
            try
            {
                await _repositoryManager.BeginTransactionAsync();

                var existingAuction = await _repositoryManager.AuctionRepo.FindAsync(a => a.Id == id, includes: new[] { "Product", "SellerUser", "BuyerUser", "Stock" });
                if (existingAuction == null) throw new NotFoundException("Auction not found");

                var currentTime = DateTime.UtcNow;

                //Dont update if auction ended 
                if (existingAuction.EndTime <= currentTime)
                    throw new BusinessException("Cannot edit ended auction");

                //if auction is running -> limited
                if (existingAuction.StartTime <= currentTime)
                {
                    if (auction.StartTime != existingAuction.StartTime)
                        throw new BusinessException("Cannot change start date after auction begins");

                    if (auction.StockId != existingAuction.StockId)
                        throw new BusinessException("Cannot change stock after auction begins");

                    if (auction.Quantity != existingAuction.Quantity)
                        throw new BusinessException("Cannot change quantity after auction begins");

                    if (auction.EndTime <= currentTime)
                        throw new BusinessException("End date must be in future");

                    existingAuction.EndTime = auction.EndTime ?? existingAuction.EndTime;

                    _mapper.Map(auction, existingAuction);
                }
                //if auction not started -> ok, can edit
                else
                {
                    if (auction.StartTime <= currentTime)
                        throw new BusinessException("Start date must be in future");

                    if (auction.EndTime <= auction.StartTime)
                        throw new BusinessException("End date must be after start date");

                    if (existingAuction.StockId != auction.StockId ||
                        existingAuction.Quantity != auction.Quantity)
                    {
                        //Edit: includes?
                        var oldStock = await _repositoryManager.StockRepo.GetByIdAsync(existingAuction.StockId); 
                        oldStock.Quantity += existingAuction.Quantity;

                        _repositoryManager.StockRepo.Update(oldStock);

                        var newStock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId ?? existingAuction.StockId);
                        if (newStock == null) throw new NotFoundException("New stock not found");

                        if (newStock.Quantity < auction.Quantity)
                            throw new BusinessException("Insufficient stock for auction");

                        newStock.Quantity -= auction.Quantity ?? existingAuction.Quantity;

                        _repositoryManager.StockRepo.Update(newStock);

                    }
                    _mapper.Map(auction, existingAuction);
                    Console.WriteLine($"Incoming ProductId: {auction.ProductId}");
                    Console.WriteLine($"Existing ProductId: {existingAuction.ProductId}");
                }

                _repositoryManager.AuctionRepo.Update(existingAuction);

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();

                return _mapper.Map<AuctionDetailsDto>(existingAuction);
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<AuctionDetailsDto>> GetAllAuctionsAsync()
        {
            var result = await _repositoryManager.AuctionRepo.GetAllAsync();

            if (!result.Any())
                throw new NotFoundException("No auctions found.");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }

        public async Task<int> GetAuctionCountAsync() => throw new NotImplementedException();

        public async Task<int> GetAuctionCountAsync(Expression<Func<Auction, bool>> filter)
        {
            if (filter == null)
                throw new NullObjectParameterException(nameof(filter));

            return await _repositoryManager.AuctionRepo.CountAsync(filter);
        }

        public async Task<AuctionDetailsDto> GetAuctionDetailsAsync(int id)
        {
            if (id <= 0)
                throw new IdParametersBadRequestException();

            var result = await _repositoryManager.AuctionRepo.FindAsync(a => a.Id == id, includes: new[] {"Product", "SellerUser", "BuyerUser", "Stock"});
            
            if (result == null)
                throw new NotFoundException("Auction not found.");

            return _mapper.Map<AuctionDetailsDto>(result);
        }

        public async Task<IEnumerable<AuctionDetailsDto>> QueryAuctionsAsync(Expression<Func<Auction, bool>> criteria, string[] includes = null)
        {
            if (criteria == null)
                throw new NullObjectParameterException(nameof(criteria));

            var result = await _repositoryManager.AuctionRepo.FindAllAsync(criteria, includes: new[] {"Product", "SellerUser", "BuyerUser" , "Stock"});
           
            if (!result.Any())
                throw new NotFoundException("No Auctions found for this criteria.");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }

        public async Task RemoveAuctionAsync(int id)
        {
            try
            {
                await _repositoryManager.BeginTransactionAsync();

                if (id <= 0)
                    throw new IdParametersBadRequestException();

                var auction = await _repositoryManager.AuctionRepo.FindAsync(a => a.Id == id, includes: new[] { "Product", "SellerUser", "BuyerUser" ,"Stock" });
                if (auction == null) throw new NotFoundException("Auction not found");

                //if auction is active -> return el stock
                if (DateTime.UtcNow < auction.EndTime)
                {
                    var stock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId);
                    stock.Quantity += auction.Quantity;

                    _repositoryManager.StockRepo.Update(stock);
                }

                _repositoryManager.AuctionRepo.Delete(auction);

                await _repositoryManager.CompleteAsync();

                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CloseEndedAuctionsAsync()
        {
            await _repositoryManager.BeginTransactionAsync();
            try
            {

                var allAuctions = await _repositoryManager.AuctionRepo.GetAllAsync();

                var now = DateTime.UtcNow;

                var endedAuctions = allAuctions.Where(a => !a.IsClosed && a.EndTime <= now && a.BuyerId != null).ToList();

                var allBids = await _repositoryManager.AuctionBidRequestRepo.GetAllAsync();

                foreach (var auction in endedAuctions)
                {
                    //get the winning bid
                    var winningBid = allBids
                        .Where(b => b.AuctionId == auction.Id && b.BidAmount == auction.CurrentBid)
                        .OrderByDescending(b => b.BidAmount)
                        .FirstOrDefault();

                    if (winningBid == null)
                        continue;


                    var existingOrders = await _repositoryManager.AuctionOrderRepo.GetAllAsync();

                    bool alreadyOrdered = existingOrders.Any(o => o.AuctionId == auction.Id);

                    if (alreadyOrdered)
                        continue;

                    var order = new AuctionOrder
                    {
                        AuctionId = auction.Id,
                        AuctionRequestId = winningBid.Id,
                        OrderDate = DateTime.UtcNow,
                        Status = OrderStatus.Pending
                    };

                    await _repositoryManager.AuctionOrderRepo.AddAsync(order);


                    auction.IsClosed = true;
                }

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task<IEnumerable<AuctionDetailsDto>> SearchAuctionsAsync(
            Expression<Func<Auction, bool>> filter,
            int? skip,
            int? take,
            Expression<Func<Auction, object>> orderBy = null,
            string orderByDirection = "ASC")
        {
            if (filter == null)
                throw new NullObjectParameterException(nameof(filter));

            var result = await _repositoryManager.AuctionRepo.FindAllAsync(
                filter,
                skip,
                take,
                null,
                orderBy,
                orderByDirection);

            if (!result.Any())
                throw new NotFoundException("No Auctions found for filter");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }

        private void ValidateAuction(Auction auction)
        {
            if (auction.Quantity <= 0)
                throw new BusinessException("Quantity must be positive");

            if (auction.StartingPrice <= 0)
                throw new BusinessException("Starting price must be positive");

            if (auction.StartTime <= DateTime.UtcNow)
                throw new BusinessException("Start date must be in future");

            if (auction.EndTime <= auction.StartTime)
                throw new BusinessException("End date must be after start date");
        }
    }


}
