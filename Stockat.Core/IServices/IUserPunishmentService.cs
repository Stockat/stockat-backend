using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserPunishmentDTOs;

namespace Stockat.Core.IServices;

public interface IUserPunishmentService
{
    Task<GenericResponseDto<PunishmentReadDto>> CreatePunishmentAsync(CreatePunishmentDto dto);
    Task<GenericResponseDto<PunishmentReadDto>> GetPunishmentByIdAsync(int id);
    Task<GenericResponseDto<IEnumerable<PunishmentReadDto>>> GetUserPunishmentsAsync(string userId);
    Task<GenericResponseDto<IEnumerable<PunishmentReadDto>>> GetAllPunishmentsAsync(int page = 1, int size = 10);
    Task<GenericResponseDto<string>> RemovePunishmentAsync(int id);
    Task<GenericResponseDto<bool>> IsUserBlockedAsync(string userId);
    Task<GenericResponseDto<PunishmentReadDto>> GetCurrentPunishmentAsync(string userId);
    
    // Additional admin methods
    Task<GenericResponseDto<object>> GetPunishmentStatisticsAsync();
    Task<GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>> GetActivePunishmentsAsync(int page = 1, int size = 10);
    Task<GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>> GetPunishmentsByTypeAsync(string type, int page = 1, int size = 10);
} 