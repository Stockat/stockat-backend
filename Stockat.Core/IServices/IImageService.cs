using Microsoft.AspNetCore.Http;
using Stockat.Core.DTOs.MediaDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IImageService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder = "/Stockat");
    Task<IEnumerable<ImageUploadResultDto>> UploadImagesAsync(IFormFile[] files, string folder = "/Stockat");
    Task<bool> DeleteImageAsync(string fileId);
}

