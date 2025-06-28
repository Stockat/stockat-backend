using Microsoft.AspNetCore.Http;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.DTOs.ServiceDTOs;

namespace Stockat.Core.IServices;

public interface IServiceService
{
    Task<IEnumerable<ServiceDto>> GetAllAvailableServicesAsync();
    Task<IEnumerable<ServiceDto>> GetSellerServicesAsync(string sellerId);
    Task<ServiceDto> GetServiceByIdAsync(int serviceId);
    Task<ServiceDto> CreateAsync(CreateServiceDto dto, string sellerId);
    Task<ServiceDto> UpdateAsync(int serviceId, UpdateServiceDto dto, string sellerId);
    Task DeleteAsync(int serviceId, string sellerId);
    public Task<ImageUploadResultDto> UploadServiceImageAsync(int serviceId, string sellerId, IFormFile file);

}
