using AutoMapper;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Stockat.Core;
using Stockat.Core.Consts;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IRepositories;
using Stockat.Core.IServices;
using Stockat.Core.IServices.IAuctionServices;
using Stockat.Core.Shared;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Stockat.Service.Services.AuctionServices
{
    public class AuctionService : IAuctionService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IServiceManager _serviceManager;


        public AuctionService(IMapper mapper,
            ILoggerManager logger,
            IRepositoryManager repositoryManager,
            IServiceManager serviceManager)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
            _serviceManager = serviceManager;
        }

        public async Task<AuctionDetailsDto> AddAuctionAsync(Auction auction)
        {
            if (auction == null)
                throw new NullObjectParameterException(nameof(auction));

            try
            {
                await _repositoryManager.BeginTransactionAsync();

                ValidateAuction(auction);

                var stock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId);
                if (stock == null) throw new NotFoundException("Stock not found");

                // Check if stock is available for auction (ForSale status)
                if (stock.StockStatus != StockStatus.ForSale)
                    throw new BusinessException("Stock is not available for auction");

                // Change stock status to SoldOut when auction is created
                stock.StockStatus = StockStatus.SoldOut;
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
                    // Only validate start time if it's actually being changed
                    if (auction.StartTime.HasValue && auction.StartTime != existingAuction.StartTime)
                        throw new BusinessException("Cannot change start date after auction begins");

                    // Only validate stock if it's actually being changed
                    if (auction.StockId.HasValue && auction.StockId != existingAuction.StockId)
                        throw new BusinessException("Cannot change stock after auction begins");

                    // Only validate end time if it's actually being changed
                    if (auction.EndTime.HasValue)
                    {
                        if (auction.EndTime <= currentTime)
                            throw new BusinessException("End date must be in future");
                        existingAuction.EndTime = auction.EndTime.Value;
                    }

                    // Update only the fields that are provided and allowed
                    if (!string.IsNullOrEmpty(auction.Name))
                        existingAuction.Name = auction.Name;
                    if (!string.IsNullOrEmpty(auction.Description))
                        existingAuction.Description = auction.Description;
                }
                //if auction not started -> ok, can edit
                else
                {
                    // Only validate start time if it's actually being changed
                    if (auction.StartTime.HasValue)
                    {
                        if (auction.StartTime <= currentTime)
                            throw new BusinessException("Start date must be in future");
                        existingAuction.StartTime = auction.StartTime.Value;
                    }

                    // Only validate end time if it's actually being changed
                    if (auction.EndTime.HasValue)
                    {
                        if (auction.EndTime <= (auction.StartTime ?? existingAuction.StartTime))
                            throw new BusinessException("End date must be after start date");
                        existingAuction.EndTime = auction.EndTime.Value;
                    }

                    // Only validate stock if it's actually being changed
                    if (auction.StockId.HasValue && auction.StockId != existingAuction.StockId)
                    {
                        // Restore old stock status to ForSale
                        var oldStock = await _repositoryManager.StockRepo.GetByIdAsync(existingAuction.StockId); 
                        oldStock.StockStatus = StockStatus.ForSale;
                        _repositoryManager.StockRepo.Update(oldStock);

                        // Check if new stock is available for auction
                        var newStock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId.Value);
                        if (newStock == null) throw new NotFoundException("New stock not found");

                        if (newStock.StockStatus != StockStatus.ForSale)
                            throw new BusinessException("New stock is not available for auction");

                        // Change new stock status to SoldOut
                        newStock.StockStatus = StockStatus.SoldOut;
                        _repositoryManager.StockRepo.Update(newStock);
                        existingAuction.StockId = auction.StockId.Value;
                    }

                    // Update other fields if provided
                    if (!string.IsNullOrEmpty(auction.Name))
                        existingAuction.Name = auction.Name;
                    if (!string.IsNullOrEmpty(auction.Description))
                        existingAuction.Description = auction.Description;
                    if (auction.StartingPrice.HasValue)
                        existingAuction.StartingPrice = auction.StartingPrice.Value;
                    if (auction.Quantity.HasValue)
                        existingAuction.Quantity = auction.Quantity.Value;
                    //if (auction.IncrementUnit.HasValue)
                    //    existingAuction.IncrementUnit = auction.IncrementUnit.Value;
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

                var auction = await _repositoryManager.AuctionRepo.FindAsync(
                    a => a.Id == id,
                    includes: new[] { "Product", "SellerUser", "BuyerUser", "Stock" }
                );

                if (auction == null)
                    throw new NotFoundException("Auction not found");

                //if auction is active, restore stock status to ForSale
                if (DateTime.UtcNow < auction.EndTime)
                {
                    var stock = await _repositoryManager.StockRepo.GetByIdAsync(auction.StockId);
                    stock.StockStatus = StockStatus.ForSale;
                    _repositoryManager.StockRepo.Update(stock); // Ensure update is saved
                }

                auction.IsDeleted = true;
                _repositoryManager.AuctionRepo.Update(auction);

                await _repositoryManager.CompleteAsync();
                await _repositoryManager.CommitTransactionAsync();
            }
            catch (Exception)
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
                var allAuctions = await _repositoryManager.AuctionRepo.FindAllAsync(
                    criteria: a => true,
                    includes: new[] { "Product", "Stock", "AuctionBidRequest", "AuctionOrder", "SellerUser" }
                );
                //a.EndTime.ToUniversalTime() <= DateTime.UtcNow

                var now = DateTime.UtcNow;

                var endedAuctions = allAuctions.Where(a => !a.IsClosed && a.EndTime.ToUniversalTime() <= now).ToList();


                var allBids = await _repositoryManager.AuctionBidRequestRepo.GetAllAsync();

                foreach (var auction in endedAuctions)
                {
                    //get the winning bid
                    var winningBid = allBids
                        .Where(b => b.AuctionId == auction.Id && b.BidAmount == auction.CurrentBid)
                        .OrderByDescending(b => b.BidAmount)
                        .FirstOrDefault();

                    if (winningBid == null)
                    {
                        // No winner: restore stock status to ForSale
                        auction.Stock.StockStatus = StockStatus.ForSale;
                        _repositoryManager.StockRepo.Update(auction.Stock); // Ensure update is saved
                        auction.IsClosed = true; // Mark auction as closed
                        _repositoryManager.AuctionRepo.Update(auction); // Save auction state
                        continue;
                    }


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
                    _repositoryManager.AuctionRepo.Update(auction);//i thinl you must call updatefrom auctio repo 

                    // Link back to auction and bid
                    auction.BuyerId = winningBid.BidderId;
                    auction.AuctionOrder = order;
                    winningBid.AuctionOrder = order;

                    // Create Stripe session
                    var session = await CreateStripeSession(order, auction);
                    order.StripeSessionId = session.Id;

                    // Send payment email
                    await SendPaymentEmail(auction, winningBid, session.Url);

                    //send email
                    var buyer = (await _repositoryManager.UserRepo.FindAllAsync(u => u.Id == winningBid.BidderId)).FirstOrDefault();
                    var seller = (await _repositoryManager.UserRepo.FindAllAsync(u => u.Id == auction.SellerId)).FirstOrDefault();

                    string subject = $"Auction Result: {auction.Name}";
                    string sellerMsg = $"<p>The auction <strong>{auction.Name}</strong> has been finalized. The winning bid was <strong>{winningBid.BidAmount:C}</strong>. You can now process the order.</p>";
                    string buyerMsg = $"<p>Congratulations! You have won the auction <strong>{auction.Name}</strong> with a bid of <strong>{winningBid.BidAmount:C}</strong>. Please proceed to payment and shipping.</p>";

                    //if (buyer != null)
                    //    await _serviceManager.EmailService.SendEmailAsync(buyer.Email, subject, buyerMsg);
                    if (seller != null)
                        await _serviceManager.EmailService.SendEmailAsync(seller.Email, subject, sellerMsg);
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
            if (auction.StartingPrice <= 0)
                throw new BusinessException("Starting price must be positive");

            if (auction.StartTime <= DateTime.UtcNow)
                throw new BusinessException("Start date must be in future");

            if (auction.EndTime <= auction.StartTime)
                throw new BusinessException("End date must be after start date");
        }


        private async Task<Session> CreateStripeSession(AuctionOrder order, Auction auction)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = $"http://localhost:4200/auction-orders/success/",
                CancelUrl = $"http://localhost:4200/auction-orders/?canceled=true",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(auction.CurrentBid * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = auction.Name ?? "Auction Item",
                            }
                        },
                        Quantity = auction.Quantity,
                    }
                },
                Mode = "payment",
                Metadata = new Dictionary<string, string>
                {
                        { "orderId", order.Id.ToString() },
                        { "type", "auction_order" }
                }
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public async Task<PagedResponse<AuctionDetailsDto>> GetAllAuctionsAsync(int pageNumber, int pageSize)
        {
            Expression<Func<Auction, bool>> filter = a => true; // No filtering, return all

            var auctions = await _repositoryManager.AuctionRepo
                .FindAllAsync(filter, skip: (pageNumber - 1) * pageSize, take: pageSize,
                    includes: new[] { "Stock" },
                    orderBy: a => a.StartTime,
                    orderByDirection: OrderBy.Descending);

            var totalCount = await _repositoryManager.AuctionRepo.CountAsync(filter);

            var data = _mapper.Map<List<AuctionDetailsDto>>(auctions);

            return new PagedResponse<AuctionDetailsDto>(data, pageNumber, pageSize, totalCount);
        }

       
        private async Task SendPaymentEmail(Auction auction, AuctionBidRequest winningBid, string paymentUrl)
        {
            var buyer = await _repositoryManager.UserRepo.GetByIdAsync(winningBid.BidderId);
            if (buyer == null) return;

            var subject = $"Payment Required: You Won {auction.Name}";
                 var message = $@"
                <p>Congratulations! You've won the auction for <strong>{auction.Name}</strong> with a bid of <strong>${winningBid.BidAmount}</strong>.</p>
                <p>Please complete your payment within 24 hours:</p>
                <p><a href='{paymentUrl}'>Complete Payment Now</a></p>
                <p>If payment is not received, your win may be forfeited.</p>
                   ";

            await _serviceManager.EmailService.SendEmailAsync(buyer.Email, subject, message);
        }
    }


}
