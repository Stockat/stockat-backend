using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserPunishmentDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Stockat.Service.Services;

public class UserPunishmentService : IUserPunishmentService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public UserPunishmentService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IUserService userService)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _userService = userService;
    }

    public async Task<GenericResponseDto<PunishmentReadDto>> CreatePunishmentAsync(CreatePunishmentDto dto)
    {
        // Validate punishment type and end date
        if (dto.Type == PunishmentType.TemporaryBan && !dto.EndDate.HasValue)
        {
            throw new BadRequestException("End date is required for temporary bans.");
        }

        if (dto.Type == PunishmentType.PermanentBan && dto.EndDate.HasValue)
        {
            throw new BadRequestException("End date should not be set for permanent bans.");
        }

        if (dto.Type == PunishmentType.TemporaryBan && dto.EndDate <= DateTime.UtcNow)
        {
            throw new BadRequestException("End date must be in the future for temporary bans.");
        }

        // Check if user exists
        var user = await _repo.UserRepo.GetByIdAsync(dto.UserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        // Check if user already has an active punishment of the same type
        var existingPunishment = await GetCurrentActivePunishmentAsync(dto.UserId);
        if (existingPunishment != null && 
            (existingPunishment.Type == PunishmentType.TemporaryBan || existingPunishment.Type == PunishmentType.PermanentBan))
        {
            throw new BadRequestException("User already has an active ban.");
        }

        var punishment = new UserPunishment
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Reason = dto.Reason,
            StartDate = DateTime.UtcNow,
            EndDate = dto.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.UserPunishmentRepo.AddAsync(punishment);
        await _repo.CompleteAsync();

        // Send email notification to user
        await SendPunishmentEmailAsync(dto.UserId, dto.Type, dto.Reason, dto.EndDate);

        var punishmentDto = _mapper.Map<PunishmentReadDto>(punishment);
        punishmentDto.UserName = $"{user.FirstName} {user.LastName}";
        punishmentDto.UserEmail = user.Email;

        return new GenericResponseDto<PunishmentReadDto>
        {
            Message = $"Punishment created successfully. Type: {dto.Type}",
            Status = StatusCodes.Status201Created,
            Data = punishmentDto
        };
    }

    public async Task<GenericResponseDto<PunishmentReadDto>> GetPunishmentByIdAsync(int id)
    {
        var punishment = await _repo.UserPunishmentRepo.FindAsync(p => p.Id == id, ["User"]);
        if (punishment == null)
            throw new NotFoundException("Punishment not found.");

        var punishmentDto = _mapper.Map<PunishmentReadDto>(punishment);
        punishmentDto.UserName = $"{punishment.User.FirstName} {punishment.User.LastName}";
        punishmentDto.UserEmail = punishment.User.Email;

        return new GenericResponseDto<PunishmentReadDto>
        {
            Message = "Punishment retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = punishmentDto
        };
    }

    public async Task<GenericResponseDto<IEnumerable<PunishmentReadDto>>> GetUserPunishmentsAsync(string userId)
    {
        var punishments = await _repo.UserPunishmentRepo.FindAllAsync(p => p.UserId == userId, ["User"]);
        
        var punishmentDtos = punishments.Select(p => 
        {
            var dto = _mapper.Map<PunishmentReadDto>(p);
            dto.UserName = $"{p.User.FirstName} {p.User.LastName}";
            dto.UserEmail = p.User.Email;
            return dto;
        });

        return new GenericResponseDto<IEnumerable<PunishmentReadDto>>
        {
            Message = "User punishments retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = punishmentDtos
        };
    }

    public async Task<GenericResponseDto<IEnumerable<PunishmentReadDto>>> GetAllPunishmentsAsync(int page = 1, int size = 10)
    {
        int skip = (page - 1) * size;
        
        var punishments = await _repo.UserPunishmentRepo.FindAllAsync(
            criteria: null,
            skip: skip,
            take: size,
            includes: ["User"]
        );

        var punishmentDtos = punishments.Select(p => 
        {
            var dto = _mapper.Map<PunishmentReadDto>(p);
            dto.UserName = $"{p.User.FirstName} {p.User.LastName}";
            dto.UserEmail = p.User.Email;
            return dto;
        });

        return new GenericResponseDto<IEnumerable<PunishmentReadDto>>
        {
            Message = "All punishments retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = punishmentDtos
        };
    }

    public async Task<GenericResponseDto<string>> RemovePunishmentAsync(int id)
    {
        var punishment = await _repo.UserPunishmentRepo.GetByIdAsync(id);
        if (punishment == null)
            throw new NotFoundException("Punishment not found.");

        _repo.UserPunishmentRepo.Delete(punishment);
        await _repo.CompleteAsync();

        return new GenericResponseDto<string>
        {
            Message = "Punishment removed successfully.",
            Status = StatusCodes.Status200OK,
            Data = id.ToString()
        };
    }

    public async Task<GenericResponseDto<bool>> IsUserBlockedAsync(string userId)
    {
        var isBlocked = await HasActivePunishmentAsync(userId);
        
        return new GenericResponseDto<bool>
        {
            Message = isBlocked ? "User is currently blocked." : "User is not blocked.",
            Status = StatusCodes.Status200OK,
            Data = isBlocked
        };
    }

    public async Task<GenericResponseDto<PunishmentReadDto>> GetCurrentPunishmentAsync(string userId)
    {
        var punishment = await GetCurrentActivePunishmentAsync(userId);
        
        if (punishment == null)
        {
            return new GenericResponseDto<PunishmentReadDto>
            {
                Message = "No active punishment found for user.",
                Status = StatusCodes.Status404NotFound,
                Data = null
            };
        }

        var punishmentDto = _mapper.Map<PunishmentReadDto>(punishment);
        punishmentDto.UserName = $"{punishment.User.FirstName} {punishment.User.LastName}";
        punishmentDto.UserEmail = punishment.User.Email;

        return new GenericResponseDto<PunishmentReadDto>
        {
            Message = "Current punishment retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = punishmentDto
        };
    }

    // Helper methods
    private async Task<UserPunishment?> GetCurrentActivePunishmentAsync(string userId)
    {
        var punishments = await _repo.UserPunishmentRepo.FindAllAsync(p => p.UserId == userId);
        
        return punishments
            .Where(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                       (p.EndDate == null || p.EndDate > DateTime.UtcNow))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();
    }

    private async Task<bool> HasActivePunishmentAsync(string userId)
    {
        var activePunishment = await GetCurrentActivePunishmentAsync(userId);
        return activePunishment != null;
    }

    private async Task SendPunishmentEmailAsync(string userId, PunishmentType type, string reason, DateTime? endDate)
    {
        try
        {
            var userResponse = await _userService.GetUserAsync(userId);
            if (userResponse?.Data == null)
            {
                _logger.LogError($"Failed to get user {userId} for punishment email notification.");
                return;
            }

            var user = userResponse.Data;
            var subject = type switch
            {
                PunishmentType.Warning => "Account Warning - Stockat",
                PunishmentType.TemporaryBan => "Account Temporarily Suspended - Stockat",
                PunishmentType.PermanentBan => "Account Permanently Banned - Stockat",
                _ => "Account Action - Stockat"
            };

            var message = type switch
            {
                PunishmentType.Warning => $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #ffc107;'>⚠️ Account Warning</h2>
                    <p>Dear {user.FirstName},</p>
                    <p>Your account has received a warning for the following reason:</p>
                    <p style='background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                        <strong>{reason}</strong>
                    </p>
                    <p>Please review our community guidelines and ensure compliance in the future.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>",

                PunishmentType.TemporaryBan => $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc3545;'>🚫 Account Temporarily Suspended</h2>
                    <p>Dear {user.FirstName},</p>
                    <p>Your account has been temporarily suspended for the following reason:</p>
                    <p style='background-color: #f8d7da; padding: 10px; border-left: 4px solid #dc3545; margin: 10px 0;'>
                        <strong>{reason}</strong>
                    </p>
                    <p>Your suspension will end on: <strong>{endDate:dd/MM/yyyy HH:mm} UTC</strong></p>
                    <p>During this period, you will not be able to access your account.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>",

                PunishmentType.PermanentBan => $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc3545;'>🚫 Account Permanently Banned</h2>
                    <p>Dear {user.FirstName},</p>
                    <p>Your account has been permanently banned for the following reason:</p>
                    <p style='background-color: #f8d7da; padding: 10px; border-left: 4px solid #dc3545; margin: 10px 0;'>
                        <strong>{reason}</strong>
                    </p>
                    <p>This decision is final and your account will no longer be accessible.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>",

                _ => $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>Account Action</h2>
                    <p>Dear {user.FirstName},</p>
                    <p>An action has been taken on your account.</p>
                    <p>Best regards,<br>Stockat Support</p>
                </div>"
            };

            await _emailService.SendEmailAsync(user.Email, subject, message);
            _logger.LogInfo($"Punishment email sent to {user.Email} for type: {type}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send punishment email to user {userId}: {ex.Message}");
        }
    }

    public async Task<GenericResponseDto<object>> GetPunishmentStatisticsAsync()
    {
        var totalPunishments = await _repo.UserPunishmentRepo.CountAsync(p => true);
        var warningCount = await _repo.UserPunishmentRepo.CountAsync(p => p.Type == PunishmentType.Warning);
        var temporaryBanCount = await _repo.UserPunishmentRepo.CountAsync(p => p.Type == PunishmentType.TemporaryBan);
        var permanentBanCount = await _repo.UserPunishmentRepo.CountAsync(p => p.Type == PunishmentType.PermanentBan);
        
        var activeBans = await _repo.UserPunishmentRepo.CountAsync(p => 
            (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
            (p.EndDate == null || p.EndDate > DateTime.UtcNow));

        var statistics = new
        {
            Total = totalPunishments,
            Warnings = warningCount,
            TemporaryBans = temporaryBanCount,
            PermanentBans = permanentBanCount,
            ActiveBans = activeBans,
            WarningPercentage = totalPunishments > 0 ? (double)warningCount / totalPunishments * 100 : 0,
            BanPercentage = totalPunishments > 0 ? (double)(temporaryBanCount + permanentBanCount) / totalPunishments * 100 : 0
        };

        return new GenericResponseDto<object>
        {
            Message = "Punishment statistics retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = statistics
        };
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>> GetActivePunishmentsAsync(int page = 1, int size = 10)
    {
        int skip = (page - 1) * size;

        var activePunishments = await _repo.UserPunishmentRepo.FindAllAsync(
            p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                 (p.EndDate == null || p.EndDate > DateTime.UtcNow),
            skip: skip,
            take: size,
            includes: ["User"]
        );

        int totalCount = await _repo.UserPunishmentRepo.CountAsync(p => 
            (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
            (p.EndDate == null || p.EndDate > DateTime.UtcNow));

        var punishmentDtos = activePunishments.Select(p => 
        {
            var dto = _mapper.Map<PunishmentReadDto>(p);
            dto.UserName = $"{p.User.FirstName} {p.User.LastName}";
            dto.UserEmail = p.User.Email;
            return dto;
        });

        var result = new PaginatedDto<IEnumerable<PunishmentReadDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = punishmentDtos
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>
        {
            Message = "Active punishments retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>> GetPunishmentsByTypeAsync(string type, int page = 1, int size = 10)
    {
        if (!Enum.TryParse<PunishmentType>(type, true, out var punishmentType))
        {
            throw new BadRequestException("Invalid punishment type.");
        }

        int skip = (page - 1) * size;

        var punishments = await _repo.UserPunishmentRepo.FindAllAsync(
            p => p.Type == punishmentType,
            skip: skip,
            take: size,
            includes: ["User"]
        );

        int totalCount = await _repo.UserPunishmentRepo.CountAsync(p => p.Type == punishmentType);

        var punishmentDtos = punishments.Select(p => 
        {
            var dto = _mapper.Map<PunishmentReadDto>(p);
            dto.UserName = $"{p.User.FirstName} {p.User.LastName}";
            dto.UserEmail = p.User.Email;
            return dto;
        });

        var result = new PaginatedDto<IEnumerable<PunishmentReadDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = punishmentDtos
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<PunishmentReadDto>>>
        {
            Message = $"Punishments of type {type} retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }
} 