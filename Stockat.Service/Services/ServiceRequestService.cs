using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly IServiceEditRequestService _serviceEditRequestService;


    public ServiceRequestService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IEmailService emailService,
        IUserService userService,
        IServiceEditRequestService serviceEditRequestService

        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _emailService = emailService;
        _userService = userService;
        _serviceEditRequestService = serviceEditRequestService;
    }

    public async Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto dto, string buyerId)
    {

        var buyer = await _repo.UserRepo.FindAsync(
            u => u.Id == buyerId,
            includes: ["UserVerification", "Punishments"]
        );

        if (buyer == null || buyer.IsDeleted || buyer.IsBlocked)
        {
            _logger.LogError($"Unauthorized buyer {buyerId} tried to create a service request.");
            throw new UnauthorizedAccessException("You are not authorized to create a service request.");
        }

        if (!buyer.IsApproved)
        {
            throw new BadRequestException("Your account is not approved yet.");
        }


        // Check for existing pending request
        var hasPending = await _repo.ServiceRequestRepo.AnyAsync(
            r => r.ServiceId == dto.ServiceId &&
                 r.BuyerId == buyerId &&
                 r.ServiceStatus == ServiceStatus.Pending
        );

        if (hasPending)
        {
            throw new BadRequestException("You already have a pending request for this service.");
        }


        // Fetch the service
        var service = await _repo.ServiceRepo.FindAsync(
            s => s.Id == dto.ServiceId && !s.IsDeleted && s.IsApproved == ApprovalStatus.Approved,
            includes: ["Seller"]);

        if (service == null)
        {
            _logger.LogError($"Service with ID {dto.ServiceId} not found.");
            throw new NotFoundException($"Service with ID {dto.ServiceId} not found.");
        }

        // Prevent buyer from creating request for their own service
        if (service.SellerId == buyerId)
        {
            _logger.LogError($"Cannot create service request for service {dto.ServiceId} by buyer {buyerId} as they are the seller.");
            throw new BadRequestException("Cannot create a service request for your own service.");
        }

        if (dto.RequestedQuantity < service.MinQuantity)
        {
            throw new BadRequestException($"Cannot request this quantity. Minimum qunatity is {service.MinQuantity}");
        }

        // Create the request
        var request = new ServiceRequest
        {
            ServiceId = dto.ServiceId,
            RequestDescription = dto.RequestDescription,
            RequestedQuantity = dto.RequestedQuantity,
            BuyerId = buyerId,
            SellerApprovalStatus = ApprovalStatus.Pending,
            BuyerApprovalStatus = ApprovalStatus.Pending,
            ServiceStatus = ServiceStatus.Pending,
            PricePerProduct = 0, // Seller sets later
            TotalPrice = 0, // Set after offer
            ImageId = service.ImageId,
            ImageUrl = service.ImageUrl,

            // SNAPSHOT FIELDS
            ServiceNameSnapshot = service.Name,
            ServiceDescriptionSnapshot = service.Description,
            ServiceMinQuantitySnapshot = service.MinQuantity,
            ServicePricePerProductSnapshot = 0, // Not set until seller offers
            ServiceEstimatedTimeSnapshot = null, // Not set until seller offers
            ServiceImageUrlSnapshot = service.ImageUrl
        };

        await _repo.ServiceRequestRepo.AddAsync(request);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Service request created successfully for service {dto.ServiceId} by buyer {buyerId}.");

        await _emailService.SendEmailAsync(
            service.Seller.Email,
            "New Service Request",
            $"You have a new service request for '{service.Name}' from {request.Buyer.FirstName} {request.Buyer.LastName}. " +
            $"Please review the request and respond accordingly."
        );

        await _emailService.SendEmailAsync(
            buyer.Email,
            "Service Request Created",
            $"Your service request for '{service.Name}' has been created successfully. " +
            $"You will be notified once the seller responds."
        );


        // Manually populate navigation properties (avoids extra query)
        request.Service = service;

        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<IEnumerable<int>> GetBuyerServiceIDsWithPendingRequests(string buyerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.BuyerId == buyerId && r.BuyerApprovalStatus == ApprovalStatus.Pending && r.ServiceStatus == ServiceStatus.Pending,
            ["Buyer", "Service"]
        );
        if (requests == null || !requests.Any())
        {
            _logger.LogInfo($"No pending service requests found for buyer {buyerId}.");
            return Enumerable.Empty<int>();
        }
        _logger.LogInfo($"Retrieved {requests.Count()} pending service requests for buyer {buyerId}.");
        return requests.Select(r => r.ServiceId).Distinct();
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>> GetBuyerRequestsAsync(string buyerId, int page, int size)
    {
        int skip = (page - 1) * size;

        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.BuyerId == buyerId,
            skip,
            size,
            includes: ["Buyer", "Service", "Service.Seller"]
        );

        int totalCount = await _repo.ServiceRequestRepo.CountAsync(r => r.BuyerId == buyerId);

        var result = new PaginatedDto<IEnumerable<ServiceRequestDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = _mapper.Map<IEnumerable<ServiceRequestDto>>(requests)
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>
        {
            Status = 200,
            Message = "Buyer requests retrieved successfully.",
            Data = result
        };
    }


    public async Task<ServiceRequestDto> GetByIdAsync(int requestId, string userId, bool isSeller)
    {
        Expression<Func<ServiceRequest, bool>> predicate = isSeller
            ? req => req.Id == requestId && (req.Service.SellerId == userId || req.BuyerId == userId)
            : req => req.Id == requestId && req.BuyerId == userId;

        var request = await _repo.ServiceRequestRepo.FindAsync(predicate, ["Buyer", "Service", "Service.Seller"]);
        if (request == null)
        {
            var role = isSeller ? "Seller" : "Buyer";
            _logger.LogError($"Service request with ID {requestId} not found for {role} {userId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for {role} {userId}.");
        }

        _logger.LogInfo($"Service request with ID {requestId} retrieved successfully for user {userId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>> GetSellerRequestsAsync(string sellerId, int serviceId, int page, int size)
    {
        int skip = (page - 1) * size;

        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceId == serviceId,
            skip,
            size,
            includes: ["Buyer", "Service"]
        );

        int totalCount = await _repo.ServiceRequestRepo.CountAsync(r => r.Service.SellerId == sellerId && r.ServiceId == serviceId);

        var result = new PaginatedDto<IEnumerable<ServiceRequestDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = _mapper.Map<IEnumerable<ServiceRequestDto>>(requests)
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>
        {
            Status = 200,
            Message = "Seller service requests retrieved successfully.",
            Data = result
        };
    }


    public async Task<ServiceRequestDto> SetSellerOfferAsync(int requestId, string sellerId, SellerOfferDto dto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.Service.SellerId == sellerId,
            ["Buyer", "Service", "Service.Seller"]
        );

        if (request == null)
            throw new NotFoundException($"Service request {requestId} not found for this seller.");

        // Check if the seller has exceeded the maximum attempts
        if (request.SellerOfferAttempts >= 3)
            throw new BadRequestException("You have reached the maximum number of offer attempts for this request.");


        if (request.SellerApprovalStatus == ApprovalStatus.Approved && request.BuyerApprovalStatus == ApprovalStatus.Pending)
            throw new BadRequestException("Waiting for buyer's response on the last offer.");

        if (request.BuyerApprovalStatus == ApprovalStatus.Rejected)
        {
            // Reset statuses to allow re-negotiation
            request.BuyerApprovalStatus = ApprovalStatus.Pending;
            request.SellerApprovalStatus = ApprovalStatus.Pending;
        }


        // Seller sets the initial offer
        request.PricePerProduct = dto.PricePerProduct;
        request.EstimatedTime = dto.EstimatedTime;
        request.TotalPrice = request.RequestedQuantity * dto.PricePerProduct;
        request.SellerApprovalStatus = ApprovalStatus.Approved;

        // Set the snapshot fields for offer
        request.ServicePricePerProductSnapshot = dto.PricePerProduct;
        request.ServiceEstimatedTimeSnapshot = dto.EstimatedTime;

        request.SellerOfferAttempts += 1;

        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        await _emailService.SendEmailAsync(
            request.Buyer.Email,
            "New Service Request Offer",
            $"Your service request for '{request.Service.Name}' has a new offer from {request.Service.Seller.FirstName} {request.Service.Seller.LastName}. " +
            $"Price per product: {dto.PricePerProduct}, Estimated time: {dto.EstimatedTime}."
        );

        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<ServiceRequestDto> UpdateBuyerStatusAsync(int requestId, string buyerId, ApprovalStatusDto statusDto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.BuyerId == buyerId,
            ["Buyer", "Service", "Service.Seller"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for buyer {buyerId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for buyer {buyerId}.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved)
        {
            _logger.LogError($"Service request with ID {requestId} has not been approved by the seller.");
            throw new BadRequestException("You cannot update the status as the seller has not approved the request.");
        }

        if (request.BuyerApprovalStatus != ApprovalStatus.Pending)
        {
            _logger.LogError($"Service request with ID {requestId} has already been updated by the buyer.");
            throw new BadRequestException("You have already updated the status for this request.");
        }

        request.BuyerApprovalStatus = statusDto.Status;

        if (statusDto.Status == ApprovalStatus.Rejected)
        {
            if (request.SellerOfferAttempts >= 3)
            {
                request.ServiceStatus = ServiceStatus.Cancelled;
            }
        }


        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        // Notify the seller about the buyer's status update
        await _emailService.SendEmailAsync(
            request.Service.Seller.Email,
            "Service Request Buyer Status Update",
            $"The buyer {request.Buyer.FirstName} {request.Buyer.LastName} has updated the status for your service request '{request.Service.Name}' to {statusDto.Status}."
        );

        _logger.LogInfo($"Buyer status for service request {requestId} updated to {statusDto.Status} by buyer {buyerId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }


    public Task<ServiceRequestDto> UpdatePaymentStatusAsync(int requestId, string paymentId, PaymentStatus status)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceRequestDto> UpdateServiceStatusAsync(int requestId, string sellerId, ServiceStatusDto dto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.Service.SellerId == sellerId,
            ["Service", "Buyer", "Service.Seller"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for seller {sellerId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for seller {sellerId}.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved || request.BuyerApprovalStatus != ApprovalStatus.Approved)
        {
            _logger.LogError($"Service request with ID {requestId} has not been approved by the seller or the buyer.");
            throw new BadRequestException("You cannot update the service status as the request has not been approved by both parties.");
        }


        request.ServiceStatus = dto.Status;
        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        // Notify the buyer about the service status update
        await _emailService.SendEmailAsync(
            request.Buyer.Email,
            "Service Request Status Update",
            $"The status of your service request '{request.Service.Name}' has been updated to {dto.Status} by the seller {request.Service.Seller.FirstName} {request.Service.Seller.LastName}."
        );

        _logger.LogInfo($"Service status for service request {requestId} updated to {dto} by seller {sellerId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<ServiceRequestDto> CancelBuyerRequest(int requestId, string buyerId)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.BuyerId == buyerId,
            ["Service"]);

        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for buyer {buyerId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for buyer {buyerId}.");
        }

        // Prevent cancellation if the request has already been approved by the seller
        if (request.SellerApprovalStatus != ApprovalStatus.Pending)
        {
            _logger.LogError($"Service request with ID {requestId} cannot be cancelled as it has already been approved by the seller.");
            throw new BadRequestException("You can only cancel requests that are still pending.");
        }

        // Set the status to cancelled
        request.ServiceStatus = ServiceStatus.Cancelled;
        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();
        _logger.LogInfo($"Service request with ID {requestId} cancelled by buyer {buyerId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }
}
