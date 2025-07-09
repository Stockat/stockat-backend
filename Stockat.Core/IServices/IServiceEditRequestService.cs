using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;

namespace Stockat.Core.IServices;

public interface IServiceEditRequestService
{
    Task SubmitEditRequestAsync(int serviceId, string sellerId, ServiceEditRequestDto dto);
    Task<List<ServiceEditRequestDto>> GetPendingRequestsAsync();
    Task ApproveEditRequestAsync(int requestId);
    Task RejectEditRequestAsync(int requestId, string note);
    Task ApplyDeferredEditsAsync(int serviceId);
}
