using Stockat.Core.DTOs.AuctionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices.IAuctionServices
{
    public interface IAuctionBidRequestService
    {
        Task<AuctionBidRequestDto> CreateBidAsync(AuctionBidRequestCreateDto dto);
        Task<AuctionBidRequestDto> GetBidByIdAsync(int id);
        public Task<IEnumerable<AuctionBidRequestDto>> GetBidsByUserIdAsync(string userId);
        Task<IEnumerable<AuctionBidRequestDto>> GetBidsByAuctionAsync(int auctionId);
        Task DeleteBidAsync(int id);
    }
}
