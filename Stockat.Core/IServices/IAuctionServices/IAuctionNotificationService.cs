using Stockat.Core.DTOs.AuctionDTOs;
using System.Threading.Tasks;

namespace Stockat.Core.IServices.IAuctionServices
{
    public interface IAuctionNotificationService
    {
        Task NotifyBidPlacedAsync(AuctionBidRequestDto bid, AuctionDetailsDto auction);
        Task NotifyAuctionClosedAsync(AuctionDetailsDto auction, string winnerId, decimal winningBid);
        Task NotifyAuctionStartedAsync(AuctionDetailsDto auction);
        Task NotifyAuctionEndingSoonAsync(AuctionDetailsDto auction);
    }
} 