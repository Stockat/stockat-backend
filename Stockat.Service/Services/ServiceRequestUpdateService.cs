using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceRequestUpdateService : IServiceRequestUpdateService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;

    public ServiceRequestUpdateService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo
        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }

    public async Task<bool> CancelUpdateAsync(int updateId, string buyerId)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && u.ServiceRequest.BuyerId == buyerId,
            ["ServiceRequest"]
        );

        if (update == null)
        {
            _logger.LogError($"Service request update {updateId} not found for buyer {buyerId}.");
            throw new NotFoundException("Service request update not found or you are not the owner.");
        }

        if (update.Status != ApprovalStatus.Pending)
        {
            _logger.LogError($"Service request update {updateId} is not in pending status.");
            throw new BadRequestException("Service request update is not in pending status.");
        }

        update.Status = ApprovalStatus.Cancelled;
        _repo.ServiceRequestUpdateRepo.Update(update);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Service request update {updateId} cancelled successfully by buyer {buyerId}.");
        return true;
    }

    public async Task<ServiceRequestUpdateDto> CreateUpdateAsync(int requestId, string buyerId, CreateServiceRequestUpdateDto dto)
    {

        var prevPendingUpdates = await _repo.ServiceRequestUpdateRepo.FindAllAsync(
            u => u.ServiceRequestId == requestId && u.Status == ApprovalStatus.Pending
        );

        if (prevPendingUpdates.Any())
        {
            _logger.LogError($"There is already a pending update for service request {requestId}.");
            throw new BadRequestException("There is already a pending update for this service request.");
        }


        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.BuyerId == buyerId,
            ["Buyer", "Service"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request {requestId} not found for buyer {buyerId}.");
            throw new NotFoundException("Service request not found.");
        }

        if(request.ServiceStatus != ServiceStatus.Pending)
        {
            _logger.LogError($"Service request {requestId} is not in pending status.");
            throw new BadRequestException("Service request is not in pending status.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved ||
                    request.BuyerApprovalStatus != ApprovalStatus.Approved)
        {
            throw new BadRequestException("Both seller and buyer must approve the request before updates can be made.");
        }

        var update = new ServiceRequestUpdate
        {
            TotalOldPrice = request.TotalPrice,
            AdditionalPrice = dto.AdditionalPrice,
            AdditionalQuantity = dto.AdditionalQuantity,
            AdditionalTime = dto.AdditionalTime,
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ServiceRequestId = requestId
        };

        await _repo.ServiceRequestUpdateRepo.AddAsync(update);
        await _repo.CompleteAsync();
        return _mapper.Map<ServiceRequestUpdateDto>(update);
    }

    public async Task<ServiceRequestDto> HandleSellerApprovalAsync(int updateId, string sellerId, bool approved)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && u.ServiceRequest.Service.SellerId == sellerId,
            ["ServiceRequest", "ServiceRequest.Service"]
        );

        if (update == null)
        {
            _logger.LogError($"Update {updateId} not found for seller {sellerId}.");
            throw new NotFoundException("Update not found or you are not the seller.");
        }


        if (update.Status != ApprovalStatus.Pending)
        {
            _logger.LogError($"Update {updateId} is not in pending status.");
            throw new BadRequestException("Update is not in pending status.");
        }


        var request = update.ServiceRequest;

        if (approved)
        {
            request.RequestedQuantity += update.AdditionalQuantity;
            request.TotalPrice = update.TotalOldPrice + (update.AdditionalQuantity * request.PricePerProduct) + update.AdditionalPrice;

            update.Status = ApprovalStatus.Approved;

            if (!string.IsNullOrEmpty(update.AdditionalTime) && !string.IsNullOrEmpty(request.EstimatedTime))
            {
                try
                {
                    var currentTime = ParseTime(request.EstimatedTime);
                    var additionalTime = ParseTime(update.AdditionalTime);

                    var totalTime = currentTime + additionalTime;
                    request.EstimatedTime = FormatTimeSpan(totalTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse or sum estimated time: {ex.Message}");
                    throw new BadRequestException("Invalid estimated time format.");
                }
            }

            _repo.ServiceRequestRepo.Update(request);
        }
        else
        {
            _logger.LogInfo($"Update {updateId} rejected by seller {sellerId}.");
            update.Status = ApprovalStatus.Rejected;
        }

        _repo.ServiceRequestUpdateRepo.Update(update);
        await _repo.CompleteAsync();

        return _mapper.Map<ServiceRequestDto>(request);
    }
       
    public async Task<ServiceRequestUpdateDto> GetUpdateByIdAsync(int updateId, string userId)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && (u.ServiceRequest.BuyerId == userId || u.ServiceRequest.Service.SellerId == userId),
            ["ServiceRequest", "ServiceRequest.Service"]
        );

        if (update == null)
        {
            _logger.LogError($"Service request update {updateId} not found.");
            throw new NotFoundException("Service request update not found.");
        }

        return _mapper.Map<ServiceRequestUpdateDto>(update);
    }

    public async Task<IEnumerable<ServiceRequestUpdateDto>> GetUpdatesByRequestIdAsync(int requestId, string userId)
    {
        var updates = await _repo.ServiceRequestUpdateRepo.FindAllAsync(
            u => u.ServiceRequestId == requestId && 
                (u.ServiceRequest.BuyerId == userId || u.ServiceRequest.Service.SellerId == userId),
            ["ServiceRequest", "ServiceRequest.Service"]
        );

        if (updates == null || !updates.Any())
        {
            _logger.LogError($"No updates found for service request {requestId}.");
            throw new NotFoundException("No updates found for this service request.");
        }

        return _mapper.Map<IEnumerable<ServiceRequestUpdateDto>>(updates);
    }

    private TimeSpan ParseTime(string timeText)
    {
        timeText = timeText.ToLower().Trim();

        if (timeText.Contains("day"))
        {
            var days = int.Parse(new string(timeText.Where(char.IsDigit).ToArray()));
            return TimeSpan.FromDays(days);
        }
        else if (timeText.Contains("week"))
        {
            var weeks = int.Parse(new string(timeText.Where(char.IsDigit).ToArray()));
            return TimeSpan.FromDays(weeks * 7);
        }

        throw new ArgumentException("Unsupported time format.");
    }

    private string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalDays >= 7 && span.TotalDays % 7 == 0)
        {
            return $"{(int)(span.TotalDays / 7)} week(s)";
        }
        else
        {
            return $"{(int)span.TotalDays} day(s)";
        }
    }


}
