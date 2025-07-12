using Stockat.Core.DTOs;
using Stockat.Core.DTOs.DriverDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices
{
    public interface IDriverService
    {
        // Add Driver
        public Task<GenericResponseDto<string>> AddDriverAsync(DriverCreateDto dto);
        public Task<GenericResponseDto<string>> UpdateDriverAsync(DriverUpdateDto dto);
        public Task<GenericResponseDto<string>> UpdateDriverStatusAsync(DriverStatusUpdateDto dto);
        public Task<GenericResponseDto<DriverDTO>> GetDriverByIdAsync(string id);
        public Task<GenericResponseDto<List<DriverDTO>>> GetAllDriversAsync();
    }
}
