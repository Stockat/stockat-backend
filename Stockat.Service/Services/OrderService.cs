using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class OrderService : IOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public OrderService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }
} 