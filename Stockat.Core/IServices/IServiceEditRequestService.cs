using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Shared;

namespace Stockat.Core.IServices;

public interface IServiceEditRequestService
{
    Task SubmitEditRequestAsync(int serviceId, string sellerId, CreateServiceEditRequestDto dto);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetPendingRequestsAsync(int page = 1, int size = 10);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetApprovedRequestsAsync(int page = 1, int size = 10);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetRejectedRequestsAsync(int page = 1, int size = 10);
    Task<GenericResponseDto<object>> GetRequestStatisticsAsync();
    Task ApproveEditRequestAsync(int requestId);
    Task RejectEditRequestAsync(int requestId, string note);
    Task ReactivateRejectedServiceAsync(int serviceId, string sellerId, CreateServiceEditRequestDto dto);
}
