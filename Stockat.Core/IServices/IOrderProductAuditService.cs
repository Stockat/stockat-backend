using Stockat.Core.DTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IOrderProductAuditService
{
    public Task<GenericResponseDto<IEnumerable<OrderProductAudit>>> getallAsync();
}
