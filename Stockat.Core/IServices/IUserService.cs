using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserDTOs;

namespace Stockat.Core.IServices;

public interface IUserService
{
    Task<GenericResponseDto<UserReadDto>> GetUserAsync(string userId = null);
    Task<GenericResponseDto<UserReadDto>> UpdateAsync(UserUpdateDto dto);
    Task<GenericResponseDto<string>> UpdateProfileImageAsync(UserImageUpdateDto dto);
    Task<GenericResponseDto<string>> ChangePasswordAsync(ChangePasswordDto dto);
    Task<GenericResponseDto<string>> ToggleActivationAsync();
    
    // Admin-specific methods
    Task<GenericResponseDto<PaginatedDto<IEnumerable<UserReadDto>>>> GetAllUsersAsync(int page = 1, int size = 10, string searchTerm = null, bool? isActive = null, bool? isVerified = null, bool? isBlocked = null);
    Task<GenericResponseDto<string>> DeactivateUserAsync(string userId);
    Task<GenericResponseDto<string>> ActivateUserAsync(string userId);
    Task<GenericResponseDto<UserReadDto>> GetUserWithDetailsAsync(string userId);
    Task<GenericResponseDto<object>> GetUserStatisticsAsync();
}
