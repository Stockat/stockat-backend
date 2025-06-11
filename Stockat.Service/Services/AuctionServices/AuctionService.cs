using AutoMapper;
using Microsoft.Extensions.Logging;
using Stockat.Core.DTOs.AuctionDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Exceptions;
using Stockat.Core.IRepositories;
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
        private readonly IAuctionRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuctionService> _logger;

        public AuctionService(IAuctionRepository repository, IMapper mapper, ILogger<AuctionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuctionDetailsDto> AddAuctionAsync(Auction auction)
        {
            if (auction == null)
                throw new NullObjectParameterException(nameof(auction));

            var result = await _repository.AddAsync(auction);

            return _mapper.Map<AuctionDetailsDto>(result);
        }

        public async Task<AuctionDetailsDto> EditAuctionAsync(int id, Auction auction)
        {
            if (auction == null)
                throw new NullObjectParameterException(nameof(auction));

            var existingAuction = await _repository.FindAsync(a => a.Id == id, includes: new[] {"Product", "SellerUser", "BuyerUser" });

            if (existingAuction == null) throw new NotFoundException("Auction not found.");

            _mapper.Map(auction, existingAuction);

            var updated = _repository.Update(existingAuction);

            return _mapper.Map<AuctionDetailsDto>(updated);
        }

        public async Task<IEnumerable<AuctionDetailsDto>> GetAllAuctionsAsync()
        {
            var result = await _repository.GetAllAsync();

            if (!result.Any())
                throw new NotFoundException("No auctions found.");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }

        public async Task<int> GetAuctionCountAsync() => await _repository.CountAsync();

        public async Task<int> GetAuctionCountAsync(Expression<Func<Auction, bool>> filter)
        {
            if (filter == null)
                throw new NullObjectParameterException(nameof(filter));

            return await _repository.CountAsync(filter);
        }

        public async Task<AuctionDetailsDto> GetAuctionDetailsAsync(int id)
        {
            if (id <= 0)
                throw new IdParametersBadRequestException();

            var result = await _repository.FindAsync(a => a.Id == id, includes: new[] {"Product", "SellerUser", "BuyerUser" });
            
            if (result == null)
                throw new NotFoundException("Auction not found.");

            return _mapper.Map<AuctionDetailsDto>(result);
        }

        public async Task<IEnumerable<AuctionDetailsDto>> QueryAuctionsAsync(Expression<Func<Auction, bool>> criteria, string[] includes = null)
        {
            if (criteria == null)
                throw new NullObjectParameterException(nameof(criteria));

            var result = await _repository.FindAllAsync(criteria, includes: new[] {"Product", "SellerUser", "BuyerUser" });
           
            if (!result.Any())
                throw new NotFoundException("No Auctions found for this criteria.");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }

        public async Task RemoveAuctionAsync(int id)
        {
            if (id <= 0)
                throw new IdParametersBadRequestException();

            var result = await _repository.FindAsync(a => a.Id == id, includes: new[] {"Product", "SellerUser", "BuyerUser" });
            
            if (result == null)
                throw new NotFoundException("Auction not found.");

            _repository.Delete(result);
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

            var result = await _repository.FindAllAsync(filter, skip, take, orderBy, orderByDirection);
            
            if (!result.Any())
                throw new NotFoundException("No Auctions found for this filter");

            return _mapper.Map<IEnumerable<AuctionDetailsDto>>(result);
        }
    }
}
