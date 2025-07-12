using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class OrderProductAuditService : IOrderProductAuditService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;

    public OrderProductAuditService(ILoggerManager logger, IMapper mapper, IRepositoryManager repo)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }

    public async Task<GenericResponseDto<IEnumerable<OrderProductAudit>>> getallAsync()
    {
        var res = await _repo.OrderProductAudit.GetAllAsync();
        return new GenericResponseDto<IEnumerable<OrderProductAudit>>()
        {

            Data = res,
            Message = "Data Fetched Successfully",
            Status = 200,
        };
    }
}
