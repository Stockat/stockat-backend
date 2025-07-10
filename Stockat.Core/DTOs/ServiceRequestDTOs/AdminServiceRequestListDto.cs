using System.Collections.Generic;

namespace Stockat.Core.DTOs.ServiceRequestDTOs
{
    public class AdminServiceRequestListDto
    {
        public PaginatedDto<IEnumerable<ServiceRequestDto>> Paginated { get; set; }
        public ServiceRequestStatsDto Stats { get; set; }
    }
} 