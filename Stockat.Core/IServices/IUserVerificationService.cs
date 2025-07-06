using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserVerificationDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Stockat.Core.IServices;

public interface IUserVerificationService
{
    Task<GenericResponseDto<UserVerificationReadDto>> GetByUserIdAsync(string userId);
    Task<GenericResponseDto<UserVerificationReadDto>> CreateAsync(UserVerificationCreateDto dto);
    Task<GenericResponseDto<UserVerificationReadDto>> UpdateAsync(UserVerificationUpdateDto dto);
    Task<GenericResponseDto<string>> DeleteAsync(string userId = null);
    Task<GenericResponseDto<UserVerificationReadDto>> UpdateStatusByAdminAsync(UserVerificationStatusUpdateDto dto);
}
