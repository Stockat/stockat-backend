using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserDTOs;

namespace Stockat.Core.IServices;

public interface IUserService
{
    Task<GenericResponseDto<UserReadDto>> GetCurrentUserAsync();
    Task<GenericResponseDto<UserReadDto>> UpdateAsync(UserUpdateDto dto);
    Task<GenericResponseDto<string>> UpdateProfileImageAsync(UserImageUpdateDto dto);
    Task<GenericResponseDto<string>> ChangePasswordAsync(ChangePasswordDto dto);
    Task<GenericResponseDto<string>> ToggleActivationAsync();
}
