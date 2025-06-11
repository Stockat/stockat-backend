using Imagekit.Sdk;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stockat.Core.DTOs.MediaDTOs;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Stockat.Service.Services;

public class ImageKitService : IImageService
{
    private readonly ImagekitClient _client;

    public ImageKitService(IConfiguration configuration)
    {
        // initialize the ImageKit client using credentials from configuration (appsettings.json)
        _client = new ImagekitClient(
            publicKey: configuration["ImageKit:PublicKey"],
            privateKey: configuration["ImageKit:PrivateKey"],
            urlEndPoint: configuration["ImageKit:UrlEndpoint"]
        );
    }

    // upload a single image file 
    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder = "/Stockat")
    {
        // validate input file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file provided.");
        }

        if (file.Length < 1024)
        {
            throw new ArgumentException("File is too small to be a valid image.");
        }

        // convert IFormFile to byte array before upload
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();

        // prepare upload request with file bytes and metadata
        var uploadFileRequest = new FileCreateRequest
        {
            file = fileBytes, // use byte array instead of stream
            fileName = Path.GetFileName(file.FileName),
            useUniqueFileName = true,
            folder = folder,
        };

        // upload the image using ImageKit SDK
        var result = await _client.UploadAsync(uploadFileRequest);

        // check for successful upload response
        if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
        {
            return new ImageUploadResultDto { FileId = result.fileId, Url = result.url };
        }

        // throw exception if upload failed
        throw new Exception($"Image upload failed: {result.Raw}");
    }



    // upload multiple image files 
    public async Task<IEnumerable<ImageUploadResultDto>> UploadImagesAsync(IFormFile[] files, string folder = "/Stockat")
    {
        var results = new List<ImageUploadResultDto>();

        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder);
                results.Add(result);
        }

        return results;
    }

    // delete an image by its fileId 
    public async Task<bool> DeleteImageAsync(string fileId)
    {
        var result = await _client.DeleteFileAsync(fileId);

        
        return result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;
    }

}
