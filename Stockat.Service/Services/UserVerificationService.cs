using AutoMapper;
using Microsoft.Extensions.Configuration;
using Stockat.Core.DTOs.UserVerificationDTOs;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using Stockat.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Stockat.Core.Exceptions;
using Stockat.Core.DTOs;

namespace Stockat.Service.Services;
public class UserVerificationService : IUserVerificationService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IImageService _imageService;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserVerificationService(
        ILoggerManager logger,
        IMapper mapper,
        IConfiguration configuration,
        IImageService imageService,
        IRepositoryManager repo,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mapper = mapper;
        _configuration = configuration;
        _imageService = imageService;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponseDto<UserVerificationReadDto>> CreateAsync(UserVerificationCreateDto dto)
    {
        var userId = GetCurrentUserId();

        var existing = await _repo.UserVerificationRepo.FindAsync(v => v.UserId == userId);
        if (existing != null)
        {
            throw new InvalidOperationException("User already has a verification entry.");
        }

        var uploadResult = await _imageService.UploadImageAsync(dto.Image, "UserDocs");
        var entity = new UserVerification
        {
            UserId = userId,
            NationalId = dto.NationalId,
            ImageId = uploadResult.FileId,
            ImageURL = uploadResult.Url,
            CreatedAt = DateTime.UtcNow,
            Status = VerificationStatus.Pending,
        };

        await _repo.UserVerificationRepo.AddAsync(entity);
        await _repo.CompleteAsync();

        return new GenericResponseDto<UserVerificationReadDto>
        {
            Message = "Verification entry created successfully.",
            Status = StatusCodes.Status201Created,
            Data = _mapper.Map<UserVerificationReadDto>(entity)
        };
    }

    public async Task<GenericResponseDto<UserVerificationReadDto>> GetByUserIdAsync(string userId)
    {
        var entity = await _repo.UserVerificationRepo.FindAsync(v => v.UserId == userId);
        if (entity == null)
        {
            return new GenericResponseDto<UserVerificationReadDto>
            {
                Message = "Verification entry not found.",
                Status = StatusCodes.Status404NotFound,
                Data = null
            };
        }

        return new GenericResponseDto<UserVerificationReadDto>
        {
            Message = "Verification entry retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<UserVerificationReadDto>(entity)
        };
    }

    public async Task<GenericResponseDto<UserVerificationReadDto>> UpdateAsync(UserVerificationUpdateDto dto)
    {
        var userId = GetCurrentUserId();

        var entity = await _repo.UserVerificationRepo.FindAsync(v => v.UserId == userId);
        if (entity == null)
            throw new NotFoundException("Verification entry not found.");

        bool isModified = false;

        if (dto.NationalId != entity.NationalId)
        {
            entity.NationalId = dto.NationalId;
            isModified = true;
        }

        if (dto.Image != null)
        {
            if (!string.IsNullOrEmpty(entity.ImageId))
                await _imageService.DeleteImageAsync(entity.ImageId);

            var uploadResult = await _imageService.UploadImageAsync(dto.Image);
            entity.ImageId = uploadResult.FileId;
            entity.ImageURL = uploadResult.Url;
            isModified = true;
        }

        if (isModified)
        {
            entity.Status = VerificationStatus.Pending;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.UserVerificationRepo.Update(entity);
            await _repo.CompleteAsync();
        }

        return new GenericResponseDto<UserVerificationReadDto>
        {
            Message = "Verification entry updated successfully.",
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<UserVerificationReadDto>(entity)
        };
    }

    public async Task<GenericResponseDto<string>> DeleteAsync(string userId = null)
    {
        userId ??= GetCurrentUserId();

        var entity = await _repo.UserVerificationRepo.FindAsync(v => v.UserId == userId);
        if (entity == null)
            throw new NotFoundException("Verification entry not found.");

        if (!string.IsNullOrEmpty(entity.ImageId))
            await _imageService.DeleteImageAsync(entity.ImageId);

        _repo.UserVerificationRepo.Delete(entity);
        await _repo.CompleteAsync();

        return new GenericResponseDto<string>
        {
            Message = "Verification entry deleted successfully.",
            Status = StatusCodes.Status200OK,
            Data = userId
        };
    }

    // helper
    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return userId;
    }
}
