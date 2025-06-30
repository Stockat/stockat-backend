using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Enums;

namespace Stockat.Core.IServices;

public interface IServiceRequestService
{
    // Buyer: Create a new service request
    Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto dto, string buyerId);

    // Buyer: Get all my requests
    Task<IEnumerable<ServiceRequestDto>> GetBuyerRequestsAsync(string buyerId);

    // Seller: Get all requests for my services
    Task<IEnumerable<ServiceRequestDto>> GetSellerRequestsAsync(string sellerId, int serviceId);

    // Shared: Get single request details
    Task<ServiceRequestDto> GetByIdAsync(int requestId, string userId, bool isSeller);

    // Seller: Update seller approval status
    Task<ServiceRequestDto> SetSellerOfferAsync(int requestId, string sellerId, SellerOfferDto offerDto);

    // Buyer: Update buyer approval status
    Task<ServiceRequestDto> UpdateBuyerStatusAsync(int requestId, string buyerId, ApprovalStatusDto statusDto);

    // System/Admin: Update payment info
    Task<ServiceRequestDto> UpdatePaymentStatusAsync(int requestId, string paymentId, PaymentStatus status);
    Task<ServiceRequestDto> UpdateServiceStatusAsync(int requestId, string sellerId, ServiceStatusDto dto);
    public Task<IEnumerable<int>> GetBuyerServiceIDsWithPendingRequests(string buyerId);
}
