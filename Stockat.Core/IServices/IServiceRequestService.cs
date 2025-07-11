using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Enums;

namespace Stockat.Core.IServices;

public interface IServiceRequestService
{
    // Buyer: Create a new service request
    Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto dto, string buyerId);

    // Buyer: Get all my requests
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>> GetBuyerRequestsAsync(string buyerId, int page, int size);

    // Seller: Get all requests for my services
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestDto>>>> GetSellerRequestsAsync(string sellerId, int serviceId, int page, int size);

    // Shared: Get single request details
    Task<ServiceRequestDto> GetByIdAsync(int requestId, string userId, bool isSeller);

    // Seller: Update seller approval status
    Task<ServiceRequestDto> SetSellerOfferAsync(int requestId, string sellerId, SellerOfferDto offerDto);

    // Buyer: Update buyer approval status
    Task<ServiceRequestDto> UpdateBuyerStatusAsync(int requestId, string buyerId, ApprovalStatusDto statusDto);

    // System/Admin: Update payment info
    Task<ServiceRequestDto> UpdatePaymentStatusAsync(int requestId, string paymentId, PaymentStatus status);
    Task<ServiceRequestDto> UpdateServiceStatusAsync(int requestId, string userId, bool isAdmin, ServiceStatusDto dto);
    Task<IEnumerable<int>> GetBuyerServiceIDsWithPendingRequests(string buyerId);
    Task<ServiceRequestDto> CancelBuyerRequest(int requestId, string buyerId);
    
    // Stripe Integration
    Task<GenericResponseDto<ServiceRequestDto>> CreateStripeCheckoutSessionAsync(int requestId, string buyerId);
    Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    Task<GenericResponseDto<ServiceRequestDto>> CancelServiceRequestOnPaymentFailureAsync(string sessionId);
    
    // Admin: Get all service requests (with optional status filter)
    Task<GenericResponseDto<AdminServiceRequestListDto>> GetAllRequestsForAdminAsync(int page, int size, ServiceStatus? status = null);
    
    // Invoice Generation
    Task InvoiceGeneratorAsync(int requestId);
}
