﻿using AutoMapper;
using Microsoft.Extensions.Configuration;
using Stockat.Core.DTOs.UserVerificationDTOs;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using Stockat.Core;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Stockat.Core.Exceptions;
using Stockat.Core.DTOs;
using Stockat.Core.Enums;
using System.Linq.Expressions;

namespace Stockat.Service.Services;
public class UserVerificationService : IUserVerificationService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IImageService _imageService;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public UserVerificationService(
        ILoggerManager logger,
        IMapper mapper,
        IConfiguration configuration,
        IImageService imageService,
        IRepositoryManager repo,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IUserService userService)
    {
        _logger = logger;
        _mapper = mapper;
        _configuration = configuration;
        _imageService = imageService;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _userService = userService;
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

            var uploadResult = await _imageService.UploadImageAsync(dto.Image, "UserDocs");
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

    public async Task<GenericResponseDto<UserVerificationReadDto>> UpdateStatusByAdminAsync(UserVerificationStatusUpdateDto dto)
    {
        var entity = await _repo.UserVerificationRepo.FindAsync(v => v.UserId == dto.UserId);
        if (entity == null)
            throw new NotFoundException("Verification entry not found.");

        if (entity.Status == VerificationStatus.Approved)
            throw new InvalidOperationException("Approved verifications can be updated.");

        if (!Enum.TryParse<VerificationStatus>(dto.Status, true, out var newStatus) ||
            (newStatus != VerificationStatus.Approved && newStatus != VerificationStatus.Rejected))
        {
            throw new InvalidOperationException("Invalid status. Only 'Approved' or 'Rejected' are allowed.");
        }

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.UserVerificationRepo.Update(entity);
        await _repo.CompleteAsync();

        // Send email notification to the user
        await SendVerificationStatusEmailAsync(dto.UserId, newStatus, dto.Note);

        return new GenericResponseDto<UserVerificationReadDto>
        {
            Message = $"Verification status updated to {newStatus}.",
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<UserVerificationReadDto>(entity)
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

    private async Task SendVerificationStatusEmailAsync(string userId, VerificationStatus status, string? adminNote = null)
    {
        try
        {
            var userResponse = await _userService.GetUserAsync(userId);
            if (userResponse?.Data == null)
            {
                _logger.LogError($"Failed to get user {userId} for email notification.");
                return;
            }

            var user = userResponse.Data;
            var subject = status == VerificationStatus.Approved 
                ? "Account Verification Approved - Stockat" 
                : "Account Verification Update - Stockat";

            var message = status == VerificationStatus.Approved
                ? $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #28a745;'>🎉 Congratulations, {user.FirstName}!</h2>
                    <p>Your account verification has been <strong>approved</strong> by our admin team.</p>
                    <p>You can now access all features of Stockat, including:</p>
                    <ul>
                        <li>Creating and managing products</li>
                        <li>Participating in auctions</li>
                        <li>Accessing seller features</li>
                        <li>Full platform functionality</li>
                    </ul>
                    <p>Thank you for your patience during the verification process.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>"
                : $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc3545;'>Verification Update</h2>
                    <p>Dear {user.FirstName},</p>
                    <p>We regret to inform you that your account verification has been <strong>rejected</strong>.</p>
                    <p>This may be due to:</p>
                    <ul>
                        <li>Incomplete or unclear documentation</li>
                        <li>Invalid or expired national ID</li>
                        <li>Document quality issues</li>
                    </ul>
                    {(string.IsNullOrEmpty(adminNote) ? "" : $@"
                    <p><strong>Admin Note:</strong></p>
                    <p style='background-color: #f8f9fa; padding: 10px; border-left: 4px solid #dc3545; margin: 10px 0;'>
                        {adminNote}
                    </p>")}
                    <p>Please review your verification documents and submit a new verification request with:</p>
                    <ul>
                        <li>Clear, high-quality images of your national ID</li>
                        <li>Valid and current identification documents</li>
                        <li>Complete and accurate information</li>
                    </ul>
                    <p>If you have any questions, please don't hesitate to contact our support team.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, message);
            _logger.LogInfo($"Verification status email sent to {user.Email} for status: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send verification status email to user {userId}: {ex.Message}");
            // Don't throw the exception to avoid breaking the main flow
        }
    }

    // Helper for combining expressions (since .And may not be available)
    private static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter)
        );
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<UserVerificationReadDto>>>> GetPendingVerificationsAsync(int page = 1, int size = 10)
    {
        return await GetPendingVerificationsAsync(page, size, null);
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<UserVerificationReadDto>>>> GetPendingVerificationsAsync(int page = 1, int size = 10, string searchTerm = null)
    {
        int skip = (page - 1) * size;
        Expression<Func<UserVerification, bool>> filter = v => v.Status == VerificationStatus.Pending;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            Expression<Func<UserVerification, bool>> searchFilter = v =>
                v.User.FirstName.ToLower().Contains(lowerTerm) ||
                v.User.LastName.ToLower().Contains(lowerTerm) ||
                v.User.Email.ToLower().Contains(lowerTerm) ||
                v.User.UserName.ToLower().Contains(lowerTerm);
            filter = AndAlso(filter, searchFilter);
        }

        var pendingVerifications = await _repo.UserVerificationRepo.FindAllAsync(
            filter,
            skip: skip,
            take: size,
            includes: ["User"]
        );

        int totalCount = await _repo.UserVerificationRepo.CountAsync(filter);

        var verificationDtos = pendingVerifications.Select(v => 
        {
            var dto = _mapper.Map<UserVerificationReadDto>(v);
            dto.UserName = $"{v.User.FirstName} {v.User.LastName}";
            dto.UserEmail = v.User.Email;
            return dto;
        });

        var result = new PaginatedDto<IEnumerable<UserVerificationReadDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = verificationDtos
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<UserVerificationReadDto>>>
        {
            Message = "Pending verifications retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }

    public async Task<GenericResponseDto<object>> GetVerificationStatisticsAsync()
    {
        var totalVerifications = await _repo.UserVerificationRepo.CountAsync(v => true);
        var pendingCount = await _repo.UserVerificationRepo.CountAsync(v => v.Status == VerificationStatus.Pending);
        var approvedCount = await _repo.UserVerificationRepo.CountAsync(v => v.Status == VerificationStatus.Approved);
        var rejectedCount = await _repo.UserVerificationRepo.CountAsync(v => v.Status == VerificationStatus.Rejected);

        var statistics = new
        {
            Total = totalVerifications,
            Pending = pendingCount,
            Approved = approvedCount,
            Rejected = rejectedCount,
            ApprovalRate = totalVerifications > 0 ? (double)approvedCount / totalVerifications * 100 : 0,
            RejectionRate = totalVerifications > 0 ? (double)rejectedCount / totalVerifications * 100 : 0
        };

        return new GenericResponseDto<object>
        {
            Message = "Verification statistics retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = statistics
        };
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<UserVerificationReadDto>>>> GetAllVerificationsAsync(int page = 1, int size = 10, string status = null, string searchTerm = null)
    {
        int skip = (page - 1) * size;
        Expression<Func<UserVerification, bool>> filter = v => true;

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<VerificationStatus>(status, true, out var parsedStatus))
            {
                filter = AndAlso(filter, v => v.Status == parsedStatus);
            }
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            Expression<Func<UserVerification, bool>> searchFilter = v =>
                v.User.FirstName.ToLower().Contains(lowerTerm) ||
                v.User.LastName.ToLower().Contains(lowerTerm) ||
                v.User.Email.ToLower().Contains(lowerTerm) ||
                v.User.UserName.ToLower().Contains(lowerTerm);
            filter = AndAlso(filter, searchFilter);
        }

        var verifications = await _repo.UserVerificationRepo.FindAllAsync(
            filter,
            skip: skip,
            take: size,
            includes: ["User"]
        );

        int totalCount = await _repo.UserVerificationRepo.CountAsync(filter);

        var verificationDtos = verifications.Select(v =>
        {
            var dto = _mapper.Map<UserVerificationReadDto>(v);
            dto.UserName = $"{v.User.FirstName} {v.User.LastName}";
            dto.UserEmail = v.User.Email;
            return dto;
        });

        var result = new PaginatedDto<IEnumerable<UserVerificationReadDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = verificationDtos
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<UserVerificationReadDto>>>
        {
            Message = "Verifications retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }
}
