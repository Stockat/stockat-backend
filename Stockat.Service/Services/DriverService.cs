using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.DriverDTOs;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services
{
    public class DriverService: IDriverService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repo;

        public DriverService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
        {
            _logger = logger;
            _mapper = mapper;
            _repo = repo;
        }

        // Add Driver
        public async Task<GenericResponseDto<string>> AddDriverAsync(DriverCreateDto dto)
        {
            var driver = _mapper.Map<Driver>(dto);
            driver.Id = Guid.NewGuid().ToString();
            driver.LastUpdateTime = DateTime.UtcNow;
            await _repo.DriverRepo.AddAsync(driver);
            await _repo.CompleteAsync();
            return new GenericResponseDto<string>
            {
                Status = 201,
                Message = "Driver added successfully",
                Data = driver.Id
            };
        }

        public async Task<GenericResponseDto<string>> UpdateDriverAsync(DriverUpdateDto dto)
        {
            var driver = await _repo.DriverRepo.FindAsync(d => d.Id == dto.Id);
            if (driver == null)
            {
                return new GenericResponseDto<string>
                {
                    Status = 404,
                    Message = "Driver not found"
                };
            }
            _mapper.Map(dto, driver);
            driver.LastUpdateTime = DateTime.UtcNow;
            _repo.DriverRepo.Update(driver);
            await _repo.CompleteAsync();
            return new GenericResponseDto<string>
            {
                Status = 200,
                Message = "Driver updated successfully"
            };
        }

        public async Task<GenericResponseDto<string>> UpdateDriverStatusAsync(DriverStatusUpdateDto dto)
        {
            var driver = await _repo.DriverRepo.FindAsync(d => d.Id == dto.Id);
            if (driver == null)
            {
                return new GenericResponseDto<string>
                {
                    Status = 404,
                    Message = "Driver not found"
                };
            }
            driver.Longitude = dto.Longitude;
            driver.Latitude = dto.Latitude;
            driver.Message = dto.Message;
            driver.LastUpdateTime = DateTime.UtcNow;
            _repo.DriverRepo.Update(driver);
            await _repo.CompleteAsync();
            return new GenericResponseDto<string>
            {
                Status = 200,
                Message = "Driver status updated successfully"
            };
        }

        public async Task<GenericResponseDto<DriverDTO>> GetDriverByIdAsync(string id)
        {
            var driver = await _repo.DriverRepo.FindAsync(d => d.Id == id);
            if (driver == null)
            {
                return new GenericResponseDto<DriverDTO>
                {
                    Status = 404,
                    Message = "Driver not found"
                };
            }
            var dto = _mapper.Map<DriverDTO>(driver);
            return new GenericResponseDto<DriverDTO>
            {
                Status = 200,
                Data = dto,
                Message = "Driver retrieved successfully"
            };
        }

        public async Task<GenericResponseDto<List<DriverDTO>>> GetAllDriversAsync()
        {
            var drivers = await _repo.DriverRepo.FindAllAsync(d => true);
            var dtos = _mapper.Map<List<DriverDTO>>(drivers.ToList());
            return new GenericResponseDto<List<DriverDTO>>
            {
                Status = 200,
                Data = dtos,
                Message = "Drivers retrieved successfully"
            };
        }
    }
}
