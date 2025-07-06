using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceService : IServiceService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IRepositoryManager _repo;
    private readonly IImageService _imageService;

    public ServiceService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IImageService imageService
        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _imageService = imageService;
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceDto dto, string sellerId)
    {
        var service = _mapper.Map<Stockat.Core.Entities.Service>(dto);
        service.SellerId = sellerId;

        await _repo.ServiceRepo.AddAsync(service);
        await _repo.CompleteAsync();
        _logger.LogInfo($"Service created successfully with ID: {service.Id}");

        return _mapper.Map<ServiceDto>(service);
    }

    public async Task DeleteAsync(int serviceId, string sellerId)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && s.SellerId == sellerId);

        if (service == null)
        {
            _logger.LogError($"Service with ID {serviceId} not found for seller {sellerId}.");
            throw new UnauthorizedAccessException("You do not own this service or it does not exist.");
        }

        _repo.ServiceRepo.Delete(service);
        await _repo.CompleteAsync();
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>> GetAllAvailableServicesAsync(int page, int size)
    {
        int skip = (page - 1) * size;
        int count = await _repo.ServiceRepo.CountAllAvailableServicesAsync();

        var services = await _repo.ServiceRepo.GetAllAvailableServicesWithSeller(skip, size);

        if (services == null || !services.Any())
        {
            _logger.LogInfo("No available services found.");
            return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>
            {
                Status = 200,
                Message = "No services available",
                Data = new PaginatedDto<IEnumerable<ServiceDto>>
                {
                    Page = page,
                    Size = size,
                    Count = 0,
                    PaginatedData = new List<ServiceDto>()
                }
            };
        }

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>
        {
            Status = 200,
            Message = "Services retrieved successfully",
            Data = new PaginatedDto<IEnumerable<ServiceDto>>
            {
                Page = page,
                Size = size,
                Count = count,
                PaginatedData = _mapper.Map<IEnumerable<ServiceDto>>(services)
            }
        };
    }


    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>> GetSellerServicesAsync(string sellerId, int page, int size)
    {
        int skip = page  * size;

        var services = await _repo.ServiceRepo.FindAllAsync(s => s.SellerId == sellerId, size, skip);
        var totalCount = await _repo.ServiceRepo.CountAsync(s => s.SellerId == sellerId);

        var paginated = new PaginatedDto<IEnumerable<ServiceDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = _mapper.Map<IEnumerable<ServiceDto>>(services)
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>
        {
            Status = 200,
            Message = "Seller services fetched successfully.",
            Data = paginated
        };
    }



    public async Task<ServiceDto> GetServiceByIdAsync(int serviceId)
    {
        var service = await _repo.ServiceRepo.GetByIdWithSeller(serviceId);
        if (service == null)
        {
            _logger.LogError($"Service with ID {serviceId} not found.");
            throw new NotFoundException($"Service with ID {serviceId} not found.");
        }
        return _mapper.Map<ServiceDto>(service);
    }

    public async Task<ServiceDto> UpdateAsync(int serviceId, UpdateServiceDto dto, string sellerId)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && s.SellerId == sellerId);
        if (service == null)
        {
            _logger.LogError($"Service with ID {serviceId} not found for seller {sellerId}.");
            throw new UnauthorizedAccessException("You do not own this service or it does not exist.");
        }

        _mapper.Map(dto, service);

        _repo.ServiceRepo.Update(service);
        await _repo.CompleteAsync();

        return _mapper.Map<ServiceDto>(service);
    }

    public async Task<ImageUploadResultDto> UploadServiceImageAsync(int serviceId, string sellerId, IFormFile file)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && s.SellerId == sellerId);

        if (service == null)
        {
            _logger.LogError($"Service {serviceId} not found or seller {sellerId} is not authorized.");
            throw new NotFoundException("Service not found or you are not authorized to modify it.");
        }

        var uploadResult = await _imageService.UploadImageAsync(file, "Services");

        service.ImageId = uploadResult.FileId;
        service.ImageUrl = uploadResult.Url;

        _repo.ServiceRepo.Update(service);
        await _repo.CompleteAsync();

        return uploadResult;
    }
}
