using Microsoft.AspNetCore.Http;

namespace Stockat.Core.IServices;

public interface IFileService
{
    Task<(string PublicId, string Url)> UploadFileAsync(IFormFile file);
    Task<List<(string PublicId, string Url)>> UploadFilesAsync(IEnumerable<IFormFile> files);
    Task DeleteFileAsync(string publicId);
}
