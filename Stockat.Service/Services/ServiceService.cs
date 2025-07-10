using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceService : IServiceService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
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
        var seller = await _repo.UserRepo.FindAsync(u => u.Id == sellerId, includes: ["UserVerification", "Punishments"])
        ?? throw new NotFoundException("Seller not found.");

        if (seller.IsDeleted)
            throw new BadRequestException("Your account is deleted. You cannot create a service.");

        if (!seller.IsApproved)
            throw new BadRequestException("Your account is not verified by admin yet.");

        if (seller.IsBlocked)
            throw new BadRequestException("You are currently blocked from creating services.");

        var service = _mapper.Map<Stockat.Core.Entities.Service>(dto);
        service.SellerId = sellerId;

        await _repo.ServiceRepo.AddAsync(service);
        await _repo.CompleteAsync();
        _logger.LogInfo($"Service created successfully with ID: {service.Id}");

        return _mapper.Map<ServiceDto>(service);
    }

    public async Task DeleteAsync(int serviceId, string sellerId, bool isAdmin = false)
    {
        Expression<Func<Stockat.Core.Entities.Service, bool>> expression;

        if (isAdmin)
        {
            expression = s => s.Id == serviceId;
        }
        else
        {
            expression = s => s.Id == serviceId && s.SellerId == sellerId;
        }

        var service = await _repo.ServiceRepo.FindAsync(expression, includes: ["ServiceRequests"]);

        if (service == null)
        {
            if (isAdmin)
            {
                _logger.LogError($"Service with ID {serviceId} not found for admin deletion.");
                throw new NotFoundException("Service not found.");
            }
            else
            {
                _logger.LogError($"Service with ID {serviceId} not found for seller {sellerId}.");
                throw new NotFoundException("You do not own this service or it does not exist.");
            }
        }

        bool hasOngoingRequests = service.ServiceRequests.Any(r =>
               r.ServiceStatus == ServiceStatus.Pending ||
               r.ServiceStatus == ServiceStatus.InProgress);

        if (hasOngoingRequests)
            throw new BadRequestException("Service has ongoing requests and cannot be deleted.");

        service.IsDeleted = true;

        _repo.ServiceRepo.Update(service);
        await _repo.CompleteAsync();
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>> GetAllAvailableServicesAsync(int page, int size, bool pendingOnly = false)
    {
        int skip = (page - 1) * size;
        int count = await _repo.ServiceRepo.CountAsync(s => s.IsApproved == ApprovalStatus.Approved && s.IsDeleted == false);

        var services = await _repo.ServiceRepo.GetAllAvailableServicesWithSeller(skip, size, pendingOnly);

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


    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>> GetSellerServicesAsync(string sellerId, int page, int size, bool isPublicView = false)
    {
        var seller = await _repo.UserRepo.FindAsync(
            s => s.Id == sellerId && s.IsDeleted == false,
            includes: ["UserVerification", "Punishments"]);

        if (seller == null)
            throw new NotFoundException("Seller not found.");

        if (!seller.IsApproved)
            throw new BadRequestException("Account is not verified by admin yet.");

        if (seller.IsBlocked)
            throw new BadRequestException("Account is currently blocked.");

        int skip = page * size;

        IEnumerable<Core.Entities.Service> services;
        int totalCount;

        if (isPublicView)
        {
            Expression<Func<Stockat.Core.Entities.Service, bool>> expression;
            expression = s => !s.IsDeleted && s.SellerId == sellerId && s.IsApproved == ApprovalStatus.Approved;

            services = await _repo.ServiceRepo.FindAllAsync(expression , skip, size);
            totalCount = await _repo.ServiceRepo.CountAsync(expression);
        }
        else
        {
            services = await _repo.ServiceRepo.FindAllAsync(s => s.SellerId == sellerId, skip, size, null);
            totalCount = await _repo.ServiceRepo.CountAsync(s => s.SellerId == sellerId);
        }

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



    public async Task<ServiceDto> GetServiceByIdAsync(int serviceId, string userId)
    {
        var service = await _repo.ServiceRepo.GetByIdWithSeller(serviceId);
        if (service == null)
        {
            _logger.LogError($"Service with ID {serviceId} not found.");
            throw new NotFoundException($"Service with ID {serviceId} not found.");
        }

        if(service.SellerId != userId && service.IsApproved != ApprovalStatus.Approved)
        {
            _logger.LogError($"Service with ID {serviceId} not found.");
            throw new NotFoundException($"Service with ID {serviceId} not found.");
        }

        return _mapper.Map<ServiceDto>(service);
    }

    public async Task<ServiceDto> UpdateAsync(int serviceId, UpdateServiceDto dto, string sellerId)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted && s.SellerId == sellerId);
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
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted && s.SellerId == sellerId);

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

    public async Task<ServiceDto> UpdateApprovalStatusAsync(int serviceId, bool isApproved)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted);
        
        if (service == null)
        {
            _logger.LogError($"Service with ID {serviceId} not found.");
            throw new NotFoundException($"Service with ID {serviceId} not found.");
        }

        if (service.IsApproved == ApprovalStatus.Approved)
            throw new BadRequestException("Service is already approved by admin.");

        service.IsApproved = isApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;

        _repo.ServiceRepo.Update(service);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Service {serviceId} approval status updated to {(isApproved ? "approved" : "rejected")}");

        return _mapper.Map<ServiceDto>(service);
    }

    private TimeSpan ParseTime(string timeText)
    {
        timeText = timeText.ToLower().Trim();

        if (timeText.Contains("day"))
        {
            var days = int.Parse(new string(timeText.Where(char.IsDigit).ToArray()));
            return TimeSpan.FromDays(days);
        }
        else if (timeText.Contains("week"))
        {
            var weeks = int.Parse(new string(timeText.Where(char.IsDigit).ToArray()));
            return TimeSpan.FromDays(weeks * 7);
        }
        else if (timeText.Contains("month"))
        {
            var months = int.Parse(new string(timeText.Where(char.IsDigit).ToArray()));
            return TimeSpan.FromDays(months * 30); // or 28/31 as you prefer
        }

        throw new ArgumentException("Unsupported time format.");
    }

    private string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalDays >= 30 && span.TotalDays % 30 == 0)
        {
            return $"{(int)(span.TotalDays / 30)} month(s)";
        }
        else if (span.TotalDays >= 7 && span.TotalDays % 7 == 0)
        {
            return $"{(int)(span.TotalDays / 7)} week(s)";
        }
        else
        {
            return $"{(int)span.TotalDays} day(s)";
        }
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>> GetAllServicesForAdminAsync(int page, int size, bool? includeBlockedSellers = null, bool? includeDeletedSellers = null, bool? includeDeletedServices = null)
    {
        int skip = (page - 1) * size;
        
        // Build filter expression
        Expression<Func<Stockat.Core.Entities.Service, bool>> filter = s => includeDeletedServices == true || !s.IsDeleted;
        
        // Get all services with seller information including verification and punishments
        var services = await _repo.ServiceRepo.FindAllAsync(
            filter,
            skip: skip,
            take: size,
            includes: ["Seller", "Seller.UserVerification", "Seller.Punishments"]
        );

        if (services == null || !services.Any())
        {
            _logger.LogInfo("No services found for admin.");
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

        // Apply seller status filtering if specified
        if (includeBlockedSellers.HasValue || includeDeletedSellers.HasValue)
        {
            services = services.Where(s => 
                (includeBlockedSellers.HasValue ? 
                    (includeBlockedSellers.Value || !s.Seller.Punishments.Any(p => 
                        (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) && 
                        (p.EndDate == null || p.EndDate > DateTime.UtcNow))) : true) &&
                (includeDeletedSellers.HasValue ? 
                    (includeDeletedSellers.Value || !s.Seller.IsDeleted) : true)
            ).ToList();
        }

        // Get total count based on the same filter
        int totalCount = await _repo.ServiceRepo.CountAsync(filter);

        var mappedServices = _mapper.Map<IEnumerable<ServiceDto>>(services);

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceDto>>>
        {
            Status = 200,
            Message = "All services retrieved successfully for admin",
            Data = new PaginatedDto<IEnumerable<ServiceDto>>
            {
                Page = page,
                Size = size,
                Count = totalCount,
                PaginatedData = mappedServices
            }
        };
    }
}
