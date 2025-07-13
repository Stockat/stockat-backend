using Stockat.Core.Consts;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices.IAuctionServices
{
    public interface IAuctionService
    {
        Task<AuctionDetailsDto> GetAuctionDetailsAsync(int id);

        Task<IEnumerable<AuctionDetailsDto>> GetAllAuctionsAsync();

        Task<IEnumerable<AuctionDetailsDto>> QueryAuctionsAsync(Expression<Func<Auction, bool>> criteria, string[] includes = null);

        Task<IEnumerable<AuctionDetailsDto>> SearchAuctionsAsync(
            Expression<Func<Auction, bool>> filter,
            int? skip,
            int? take,
            Expression<Func<Auction, object>> orderBy = null,
            string orderByDirection = OrderBy.Ascending);


        Task<AuctionDetailsDto> AddAuctionAsync(Auction auction);

        Task<AuctionDetailsDto> EditAuctionAsync(int id, AuctionUpdateDto auction);

        public Task<PagedResponse<AuctionDetailsDto>> GetAllAuctionsAsync(int pageNumber, int pageSize);

        Task RemoveAuctionAsync(int id);

        Task<int> GetAuctionCountAsync();

        Task<int> GetAuctionCountAsync(Expression<Func<Auction, bool>> filter);

        public Task CloseEndedAuctionsAsync();
    }


}
