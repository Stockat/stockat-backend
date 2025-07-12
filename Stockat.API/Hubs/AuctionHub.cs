using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stockat.Core.IServices;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Stockat.API.Hubs;

[Authorize]
public class AuctionHub : Hub
{
    private readonly IServiceManager _serviceManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuctionHub> _logger;

    public AuctionHub(IServiceManager serviceManager, IHttpContextAccessor httpContextAccessor, ILogger<AuctionHub> logger)
    {
        _serviceManager = serviceManager;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string GetUserId() =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.UserIdentifier;

    public async Task JoinAuction(int auctionId)
    {
        if (auctionId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid auction ID.");
            return;
        }

        try
        {
            // Verify auction exists and is active
            var auction = await _serviceManager.AuctionService.GetAuctionDetailsAsync(auctionId);
            if (auction == null)
            {
                await Clients.Caller.SendAsync("Error", "Auction not found.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
            _logger.LogInformation("[SignalR] User {ConnectionId} joined auction-{AuctionId}", Context.ConnectionId, auctionId);
            await Clients.Caller.SendAsync("JoinedAuction", auctionId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task LeaveAuction(int auctionId)
    {
        if (auctionId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid auction ID.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        await Clients.Caller.SendAsync("LeftAuction", auctionId);
    }

    public async Task PlaceBid(AuctionBidRequestCreateDto bidDto)
    {
        if (bidDto == null || bidDto.AuctionId <= 0 || bidDto.BidAmount <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid bid data.");
            return;
        }

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        _logger.LogInformation("[SignalR] PlaceBid called for auction {AuctionId} by {UserId}", bidDto.AuctionId, userId);

        try
        {
            // Set the bidder ID from the authenticated user
            bidDto.BidderId = userId;

            // Create the bid through the service
            var bid = await _serviceManager.AuctionBidRequestService.CreateBidAsync(bidDto);

            // Get updated auction details
            var auction = await _serviceManager.AuctionService.GetAuctionDetailsAsync(bidDto.AuctionId);

            // Send real-time updates to all users watching this auction
            await Clients.Group($"auction-{bidDto.AuctionId}").SendAsync("BidPlaced", new
            {
                Bid = bid,
                Auction = auction,
                Message = $"New bid placed: {bidDto.BidAmount:C}",
                Timestamp = DateTime.UtcNow
            });

            // Send success message to the bidder
            await Clients.Caller.SendAsync("BidSuccess", bid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error in PlaceBid for auction {AuctionId}", bidDto.AuctionId);
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task GetAuctionUpdates(int auctionId)
    {
        if (auctionId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid auction ID.");
            return;
        }

        try
        {
            var auction = await _serviceManager.AuctionService.GetAuctionDetailsAsync(auctionId);
            var bids = await _serviceManager.AuctionBidRequestService.GetBidsByAuctionAsync(auctionId);

            await Clients.Caller.SendAsync("AuctionUpdate", new
            {
                Auction = auction,
                Bids = bids,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task WatchAuction(int auctionId)
    {
        if (auctionId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid auction ID.");
            return;
        }

        try
        {
            var auction = await _serviceManager.AuctionService.GetAuctionDetailsAsync(auctionId);
            if (auction == null)
            {
                await Clients.Caller.SendAsync("Error", "Auction not found.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
            await Clients.Caller.SendAsync("WatchingAuction", auctionId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task StopWatchingAuction(int auctionId)
    {
        if (auctionId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid auction ID.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        await Clients.Caller.SendAsync("StoppedWatchingAuction", auctionId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
        // Clean up any auction groups the user was in
        // This would require tracking user's auction subscriptions
    }
} 