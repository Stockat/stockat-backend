using System;
using Stockat.Core.IServices;
using Stockat.Core;
using Stockat.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.StockDTOs;
using AutoMapper;

namespace Stockat.Service.Services
{
    public class StockService : IStockService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repo;

        public StockService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
        {
            _logger = logger;
            _mapper = mapper;
            _repo = repo;
        }

        public async Task<GenericResponseDto<AddStockDTO>> AddStockAsync(AddStockDTO stockDto)
        {
            // Create stock entity from DTO
            var stock = _mapper.Map<Stock>(stockDto);

            // Add stock to the repository
            await _repo.StockRepo.AddAsync(stock);
            // Save changes to the database
            await _repo.CompleteAsync();

            return new GenericResponseDto<AddStockDTO>
            {
                Message = "Stock added successfully",
                Status = 201,
                Data = stockDto
            };



        }
    }
}
