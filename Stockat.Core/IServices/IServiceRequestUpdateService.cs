using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;

namespace Stockat.Core.IServices;

public interface IServiceRequestUpdateService
{
    Task<ServiceRequestUpdateDto> CreateUpdateAsync(int requestId, string buyerId, CreateServiceRequestUpdateDto createDto);
    Task<IEnumerable<ServiceRequestUpdateDto>> GetUpdatesByRequestIdAsync(int requestId, string userId);
    Task<ServiceRequestUpdateDto> GetUpdateByIdAsync(int updateId, string userId);
    Task<bool> CancelUpdateAsync(int updateId, string sellerId);
    Task<ServiceRequestDto> HandleSellerApprovalAsync(int updateId, string sellerId, bool approved);

}
