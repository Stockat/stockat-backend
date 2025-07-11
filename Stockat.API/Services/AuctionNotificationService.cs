using Microsoft.AspNetCore.SignalR;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.IServices.IAuctionServices;
using System;
using System.Threading.Tasks;
using Stockat.API.Hubs;
using Microsoft.Extensions.Logging;

namespace Stockat.API.Services
{
    public class AuctionNotificationService : IAuctionNotificationService
    {
        private readonly IHubContext<AuctionHub> _auctionHub;
        private readonly ILogger<AuctionNotificationService> _logger;

        public AuctionNotificationService(IHubContext<AuctionHub> auctionHub, ILogger<AuctionNotificationService> logger)
        {
            _auctionHub = auctionHub;
            _logger = logger;
        }

        public async Task NotifyBidPlacedAsync(AuctionBidRequestDto bid, AuctionDetailsDto auction)
        {
            try
            {
                _logger.LogInformation("[SignalR] Sending BidPlaced for auction-{AuctionId} to group.", bid.AuctionId);
                await _auctionHub.Clients.Group($"auction-{bid.AuctionId}").SendAsync("BidPlaced", new
                {
                    Bid = bid,
                    Auction = auction,
                    Message = $"New bid placed: {bid.BidAmount:C}",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR] Error sending BidPlaced for auction-{AuctionId}", bid.AuctionId);
            }
        }

        public async Task NotifyAuctionClosedAsync(AuctionDetailsDto auction, string winnerId, decimal winningBid)
        {
            try
            {
                await _auctionHub.Clients.Group($"auction-{auction.Id}").SendAsync("AuctionClosed", new
                {
                    Auction = auction,
                    WinnerId = winnerId,
                    WinningBid = winningBid,
                    Message = $"Auction '{auction.Name}' has ended. Winning bid: {winningBid:C}",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
            }
        }

        public async Task NotifyAuctionStartedAsync(AuctionDetailsDto auction)
        {
            try
            {
                await _auctionHub.Clients.Group($"auction-{auction.Id}").SendAsync("AuctionStarted", new
                {
                    Auction = auction,
                    Message = $"Auction '{auction.Name}' has started!",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
            }
        }

        public async Task NotifyAuctionEndingSoonAsync(AuctionDetailsDto auction)
        {
            try
            {
                await _auctionHub.Clients.Group($"auction-{auction.Id}").SendAsync("AuctionEndingSoon", new
                {
                    Auction = auction,
                    Message = $"Auction '{auction.Name}' is ending soon!",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
            }
        }
    }
} 