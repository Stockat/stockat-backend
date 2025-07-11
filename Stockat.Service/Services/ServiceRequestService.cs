using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.Helpers;
using Stockat.Core.IServices;
using Stripe;
using Stripe.Checkout;

namespace Stockat.Service.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly IServiceEditRequestService _serviceEditRequestService;


    public ServiceRequestService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IEmailService emailService,
        UserManager<User> userManager,
        IServiceEditRequestService serviceEditRequestService

        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _emailService = emailService;
        _userManager = userManager;
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

    public async Task<ServiceRequestDto> UpdateServiceStatusAsync(int requestId, string userId, bool isAdmin, ServiceStatusDto dto)
    {
        ServiceRequest? request = null;
        if (isAdmin)
        {
            request = await _repo.ServiceRequestRepo.FindAsync(
                r => r.Id == requestId,
                ["Service", "Buyer", "Service.Seller"]
            );
            if (request == null)
            {
                _logger.LogError($"Service request with ID {requestId} not found for admin {userId}.");
                throw new NotFoundException($"Service request with ID {requestId} not found for admin {userId}.");
            }
            // Only allow admin to set status to Delivered if current status is InProgress
            if (request.ServiceStatus != ServiceStatus.InProgress || dto.Status != ServiceStatus.Delivered)
            {
                throw new BadRequestException("Admin can only set status to Delivered for requests that are In Progress.");
            }
        }
        else
        {
            request = await _repo.ServiceRequestRepo.FindAsync(
                r => r.Id == requestId && r.Service.SellerId == userId,
                ["Service", "Buyer", "Service.Seller"]
            );
            if (request == null)
            {
                _logger.LogError($"Service request with ID {requestId} not found for seller {userId}.");
                throw new NotFoundException($"Service request with ID {requestId} not found for seller {userId}.");
            }
            if (request.SellerApprovalStatus != ApprovalStatus.Approved || request.BuyerApprovalStatus != ApprovalStatus.Approved)
            {
                _logger.LogError($"Service request with ID {requestId} has not been approved by the seller or the buyer.");
                throw new BadRequestException("You cannot update the service status as the request has not been approved by both parties.");
            }
        }
        request.ServiceStatus = dto.Status;
        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();
        // Notify the buyer about the service status update
        await _emailService.SendEmailAsync(
            request.Buyer.Email,
            "Service Request Status Update",
            $"The status of your service request '{request.Service.Name}' has been updated to {dto.Status}."
        );
        _logger.LogInfo($"Service status for service request {requestId} updated to {dto.Status} by user {userId}.");
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

    public async Task<GenericResponseDto<AdminServiceRequestListDto>> GetAllRequestsForAdminAsync(int page, int size, ServiceStatus? status = null)
    {
        int skip = (page - 1) * size;
        Expression<Func<ServiceRequest, bool>> filter = status.HasValue
            ? r => r.ServiceStatus == status.Value
            : r => true;

        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            filter,
            skip,
            size,
            includes: ["Buyer", "Service", "Service.Seller"]
        );

        int totalCount = await _repo.ServiceRequestRepo.CountAsync(r => true);
        int readyCount = await _repo.ServiceRequestRepo.CountAsync(r => r.ServiceStatus == ServiceStatus.Ready);
        int deliveredCount = await _repo.ServiceRequestRepo.CountAsync(r => r.ServiceStatus == ServiceStatus.Delivered);

        var paginated = new PaginatedDto<IEnumerable<ServiceRequestDto>>
        {
            Page = page,
            Size = size,
            Count = status.HasValue
                ? await _repo.ServiceRequestRepo.CountAsync(r => r.ServiceStatus == status.Value)
                : totalCount,
            PaginatedData = _mapper.Map<IEnumerable<ServiceRequestDto>>(requests)
        };

        var stats = new ServiceRequestStatsDto
        {
            Total = totalCount,
            Ready = readyCount,
            Delivered = deliveredCount
        };

        var result = new AdminServiceRequestListDto
        {
            Paginated = paginated,
            Stats = stats
        };

        return new GenericResponseDto<AdminServiceRequestListDto>
        {
            Status = 200,
            Message = "All service requests retrieved successfully for admin.",
            Data = result
        };
    }

    public async Task<GenericResponseDto<ServiceRequestDto>> CreateStripeCheckoutSessionAsync(int requestId, string buyerId)
    {
        try
        {
            var request = await _repo.ServiceRequestRepo.FindAsync(
                r => r.Id == requestId && r.BuyerId == buyerId,
                ["Buyer", "Service", "Service.Seller"]
            );

            if (request == null)
            {
                _logger.LogError($"Service request {requestId} not found for buyer {buyerId}.");
                return new GenericResponseDto<ServiceRequestDto>
                {
                    Status = 404,
                    Message = "Service request not found."
                };
            }

            // Check if buyer can proceed to checkout
            if (request.SellerApprovalStatus != ApprovalStatus.Approved || 
                request.BuyerApprovalStatus != ApprovalStatus.Approved)
            {
                _logger.LogError($"Service request {requestId} is not ready for checkout.");
                return new GenericResponseDto<ServiceRequestDto>
                {
                    Status = 400,
                    Message = "Service request is not ready for checkout. Both seller and buyer must approve."
                };
            }

            // Check for pending updates
            var hasPendingUpdates = await _repo.ServiceRequestUpdateRepo.AnyAsync(
                u => u.ServiceRequestId == requestId && u.Status == ApprovalStatus.Pending
            );

            if (hasPendingUpdates)
            {
                _logger.LogError($"Service request {requestId} has pending updates.");
                return new GenericResponseDto<ServiceRequestDto>
                {
                    Status = 400,
                    Message = "Cannot proceed to checkout while there are pending request updates. Please wait for seller approval or cancel the updates."
                };
            }

            // Allow retry for failed payments, but not for completed payments
            if (request.PaymentStatus == PaymentStatus.Paid)
            {
                _logger.LogError($"Service request {requestId} has already been paid.");
                return new GenericResponseDto<ServiceRequestDto>
                {
                    Status = 400,
                    Message = "This service request has already been paid."
                };
            }

            // Create Stripe session
            var sessionItems = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(request.TotalPrice * 100), // Convert to cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = request.ServiceNameSnapshot,
                        Description = request.ServiceDescriptionSnapshot
                    }
                },
                Quantity = 1, // Service request is a single item
            };

            var options = new SessionCreateOptions
            {
                SuccessUrl = "http://localhost:4200/profile",
                CancelUrl = "http://localhost:4200/profile",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", request.Id.ToString() },
                    { "type", "service_request" }
                }
            };
            options.LineItems.Add(sessionItems);

            var service = new SessionService();
            Session session = service.Create(options);

            // Update request with session ID
            await UpdateStripePaymentID(request.Id, session.Id, session.PaymentIntentId);

            var responseDto = _mapper.Map<ServiceRequestDto>(request);

            return new GenericResponseDto<ServiceRequestDto>
            {
                Status = 201,
                Data = responseDto,
                Message = "Stripe checkout session created successfully.",
                RedirectUrl = session.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating Stripe checkout session: {ex.Message}");
            return new GenericResponseDto<ServiceRequestDto>
            {
                Status = 500,
                Message = "An error occurred while creating the checkout session."
            };
        }
    }

    public async Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
    {
        var request = await _repo.ServiceRequestRepo.GetByIdAsync(id);
        if (!string.IsNullOrEmpty(sessionId))
        {
            request.SessionId = sessionId;
        }
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            request.PaymentId = paymentIntentId;
            request.PaymentStatus = PaymentStatus.Paid;
            request.PaymentDate = DateTime.Now;
        }

        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();
    }

    public async Task<GenericResponseDto<ServiceRequestDto>> CancelServiceRequestOnPaymentFailureAsync(string sessionId)
    {
        try
        {
            var request = await _repo.ServiceRequestRepo.FindAsync(
                r => r.SessionId == sessionId,
                ["Buyer", "Service", "Service.Seller"]
            );

            if (request == null)
            {
                _logger.LogError($"Service request with Session ID {sessionId} not found.");
                return new GenericResponseDto<ServiceRequestDto>
                {
                    Status = 404,
                    Message = "Service request not found."
                };
            }

            request.PaymentStatus = PaymentStatus.Failed;
            request.SessionId = null; // Clear session to allow new payment attempt
            _repo.ServiceRequestRepo.Update(request);

            await _repo.CompleteAsync();

            // Send email notification to buyer about payment failure
            await _emailService.SendEmailAsync(
                request.Buyer.Email,
                "Service Request Payment Failed",
                $"Your payment for service request '{request.ServiceNameSnapshot}' has failed. " +
                $"You can retry the payment from your dashboard. The request remains active."
            );

            // Notify seller about payment failure
            await _emailService.SendEmailAsync(
                request.Service.Seller.Email,
                "Service Request Payment Failed",
                $"Payment failed for service request '{request.ServiceNameSnapshot}' from {request.Buyer.FirstName} {request.Buyer.LastName}. " +
                $"The buyer will be notified to retry payment."
            );

            var requestDto = _mapper.Map<ServiceRequestDto>(request);
            return new GenericResponseDto<ServiceRequestDto>
            {
                Status = 200,
                Data = requestDto,
                Message = "Payment failed. Service request remains active and can be retried."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling service request payment failure: {ex.Message}");
            return new GenericResponseDto<ServiceRequestDto>
            {
                Status = 500,
                Message = "An error occurred while handling the payment failure."
            };
        }
    }

    // Invoice Generator for Service Requests
    public async Task InvoiceGeneratorAsync(int requestId)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId, 
            ["Service", "Buyer"]
        );
        
        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for invoice generation.");
            return;
        }

        var user = await _repo.UserRepo.GetByIdAsync(request.BuyerId);
        if (user == null)
        {
            _logger.LogError($"User with ID {request.BuyerId} not found for invoice generation.");
            return;
        }

        var invoice = InvoiceGenerator.CreateInvoice(
            request.PaymentId, 
            request.PaymentDate?.ToString() ?? DateTime.Now.ToString(), 
            "Credit Card", 
            request.ServiceNameSnapshot,
            request.RequestedQuantity, 
            request.PricePerProduct, 
            "stockatgroup@gmail.com"
        );

        await _emailService.SendEmailAsync(user.Email, "Service Request Payment Receipt", invoice);
        
        _logger.LogInfo($"Invoice generated and sent for service request {requestId} to {user.Email}");
    }

    // --- SELLER ANALYTICS METHODS ---
    public async Task<object> GetSellerServiceRequestStatusBreakdownAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId,
            includes: ["Service"]);

        var breakdown = requests
            .GroupBy(r => r.ServiceStatus)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToList();
        return breakdown;
    }

    public async Task<object> GetSellerServiceRequestMonthlyTrendAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId,
            includes: ["Service"]
            );
        var trend = requests
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .ToList();
        return trend;
    }

    public async Task<object> GetSellerServiceRevenueAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceStatus == ServiceStatus.Delivered,
            includes: ["Service"]);

        var now = DateTime.UtcNow;
        var thisMonth = requests.Where(r => r.CreatedAt.Year == now.Year && r.CreatedAt.Month == now.Month).Sum(r => r.TotalPrice);
        var total = requests.Sum(r => r.TotalPrice);
        return new { TotalRevenue = total, ThisMonthRevenue = thisMonth };
    }

    public async Task<object> GetSellerTopServicesByRequestsAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId, 
            includes: ["Service"]);

        var top = requests
            .GroupBy(r => new { r.ServiceId, r.Service.Name })
            .Select(g => new { ServiceId = g.Key.ServiceId, ServiceName = g.Key.Name, RequestCount = g.Count() })
            .OrderByDescending(x => x.RequestCount)
            .Take(5)
            .ToList();
        return top;
    }

    public async Task<object> GetSellerCustomerFeedbackAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceStatus == ServiceStatus.Delivered,
            includes: ["Reviews", "Reviews.Reviewer"]
        );

        var reviews = requests.SelectMany(r => r.Reviews).ToList();
        
        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        var totalReviews = reviews.Count;
        var recentReviews = reviews.OrderByDescending(r => r.CreatedAt).Take(5).ToList();

        return new
        {
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = totalReviews,
            RecentReviews = recentReviews.Select(r => new
            {
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                BuyerName = r.Reviewer.FirstName + " " + r.Reviewer.LastName
            }).ToList()
        };
    }

    public async Task<object> GetSellerConversionFunnelAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(r => r.Service.SellerId == sellerId);
        
        var funnel = new
        {
            Pending = requests.Count(r => r.ServiceStatus == ServiceStatus.Pending),
            InProgress = requests.Count(r => r.ServiceStatus == ServiceStatus.InProgress),
            Ready = requests.Count(r => r.ServiceStatus == ServiceStatus.Ready),
            Delivered = requests.Count(r => r.ServiceStatus == ServiceStatus.Delivered),
            Cancelled = requests.Count(r => r.ServiceStatus == ServiceStatus.Cancelled)
        };

        return funnel;
    }

    public async Task<object> GetSellerServiceReviewsAsync(string sellerId)
    {
        var services = await _repo.ServiceRepo.FindAllAsync(s => s.SellerId == sellerId, includes: ["Reviews", "Reviews.Reviewer"]);
        var allReviews = services.SelectMany(s => s.Reviews).ToList();
        var averageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
        var totalReviews = allReviews.Count;
        var recentReviews = allReviews.OrderByDescending(r => r.CreatedAt).Take(5).ToList();
        return new
        {
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = totalReviews,
            RecentReviews = recentReviews.Select(r => new
            {
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                ReviewerName = r.Reviewer.FirstName + " " + r.Reviewer.LastName
            }).ToList()
        };
    }

    public async Task<object> GetSellerTopCustomersAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceStatus == ServiceStatus.Delivered,
            includes: ["Buyer", "Service"]
        );

        var topCustomers = requests
            .GroupBy(r => new { r.BuyerId, r.Buyer.FirstName, r.Buyer.LastName, r.Buyer.Email })
            .Select(g => new
            {
                CustomerId = g.Key.BuyerId,
                CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                Email = g.Key.Email,
                TotalSpent = g.Sum(r => r.TotalPrice),
                OrderCount = g.Count(),
                LastOrderDate = g.Max(r => r.CreatedAt),
                AverageOrderValue = g.Average(r => r.TotalPrice)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(10)
            .ToList();

        return topCustomers;
    }

    public async Task<object> GetSellerCustomerDemographicsAsync(string sellerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceStatus == ServiceStatus.Delivered,
            includes: ["Buyer"]
        );

        var customers = requests
            .GroupBy(r => r.BuyerId)
            .Select(g => g.First().Buyer)
            .ToList();

        // Location distribution (by city)
        var locationDistribution = customers
            .Where(c => !string.IsNullOrEmpty(c.City))
            .GroupBy(c => c.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

                return new
        {
            LocationDistribution = locationDistribution,
            TotalCustomers = customers.Count
        };
    }

  
}
