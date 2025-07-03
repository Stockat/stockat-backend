using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stockat.Core.IServices;

namespace Stockat.Infrastructure.Services;

public class CloudinaryFileService : IFileService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string PublicId, string Url)> UploadFileAsync(IFormFile file)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            UseFilename = true,
            UniqueFilename = true,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return (result.PublicId, result.SecureUrl.ToString());
    }

    public async Task<List<(string PublicId, string Url)>> UploadFilesAsync(IEnumerable<IFormFile> files)
    {
        var uploaded = new List<(string PublicId, string Url)>();

        foreach (var file in files)
        {
            if (file != null && file.Length > 0)
                uploaded.Add(await UploadFileAsync(file));
        }

        return uploaded;
    }

    public async Task DeleteFileAsync(string publicId)
    {
        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
