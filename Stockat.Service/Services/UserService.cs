using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Stockat.Service.Services;
using Stockat.Core.Helpers;
using Stockat.Core.Enums;
using System.Security.Claims;
using System.Linq.Expressions;

public class UserService : IUserService
{
    private readonly IRepositoryManager _repo;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IImageService _imageService;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;

    public UserService(
        IRepositoryManager repo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IImageService imageService,
        UserManager<User> userManager,
        IEmailService emailService)
    {
        _repo = repo;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _imageService = imageService;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<GenericResponseDto<UserReadDto>> GetUserAsync(string userId = null)
    {
        if (userId is null)
            userId = GetCurrentUserId();

        var includes = new[] { nameof(User.UserVerification) };

        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId, includes);
        if (user == null)
            throw new NotFoundException("User not found.");

        var dto = _mapper.Map<UserReadDto>(user);
        dto.IsApproved = user.IsApproved;
        dto.IsDeleted = user.IsDeleted;

        var roles = await _userManager.GetRolesAsync(user);
        dto.Roles = roles.ToList();

        return new GenericResponseDto<UserReadDto>
        {
            Message = "User retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = dto
        };
    }



    public async Task<GenericResponseDto<UserReadDto>> UpdateAsync(UserUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Address = dto.Address;
        user.City = dto.City;
        user.Country = dto.Country;
        user.PostalCode = dto.PostalCode;
        user.AboutMe = dto.AboutMe;
        user.PhoneNumber = dto.PhoneNumber;
        _repo.UserRepo.Update(user);
        await _repo.CompleteAsync();


        var includes = new[] { nameof(User.UserVerification) };

        user = await _repo.UserRepo.FindAsync(u => u.Id == userId, includes);
        if (user == null)
            throw new NotFoundException("User not found.");

        var tempDto = _mapper.Map<UserReadDto>(user);
        tempDto.IsApproved = user.IsApproved;
        tempDto.IsDeleted = user.IsDeleted;

        var roles = await _userManager.GetRolesAsync(user);
        tempDto.Roles = roles.ToList();

        return new GenericResponseDto<UserReadDto>
        {
            Message = "User updated successfully.",
            Status = StatusCodes.Status200OK,
            Data = tempDto
        };
    }

    public async Task<GenericResponseDto<string>> UpdateProfileImageAsync(UserImageUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.ProfileImageId))
            await _imageService.DeleteImageAsync(user.ProfileImageId);

        var uploadResult = await _imageService.UploadImageAsync(dto.ProfileImage, "/UserProfiles");
        user.ProfileImageId = uploadResult.FileId;
        user.ProfileImageUrl = uploadResult.Url;

        _repo.UserRepo.Update(user);
        await _repo.CompleteAsync();

        return new GenericResponseDto<string>
        {
            Message = "Profile image updated successfully.",
            Status = StatusCodes.Status200OK,
            Data = user.ProfileImageUrl
        };
    }

    public async Task<GenericResponseDto<string>> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return new GenericResponseDto<string>
            {
                Message = "Password change failed",
                Status = StatusCodes.Status400BadRequest,
                Data = errors
            };
        }

        var emailBody = $@"
        <p>Your password was successfully changed.</p>
        <p>If you did not perform this action, please <a href='mailto:stockatgroup@gmail.com'>contact support</a> immediately.</p>";

        await _emailService.SendEmailAsync(user.Email, "Password Changed", emailBody);

        return new GenericResponseDto<string>
        {
            Message = "Password changed successfully.",
            Status = StatusCodes.Status200OK,
            Data = "Success"
        };
    }

    //public async Task<GenericResponseDto<string>> DeactivateAsync()
    //{
    //    var userId = GetCurrentUserId();

    //    var user = await _repo.UserRepo.FindAsync(u => u.Id == userId);
    //    if (user == null)
    //        throw new NotFoundException("User not found.");

    //    if (user.IsDeleted)
    //    {
    //        return new GenericResponseDto<string>
    //        {
    //            Message = "User is already deactivated.",
    //            Status = StatusCodes.Status400BadRequest,
    //            Data = "Already deactivated"
    //        };
    //    }

    //    user.IsDeleted = true;

    //    _repo.UserRepo.Update(user);
    //    await _repo.CompleteAsync();

    //    return new GenericResponseDto<string>
    //    {
    //        Message = "User deactivated successfully.",
    //        Status = StatusCodes.Status200OK,
    //        Data = user.Id
    //    };
    //}

    public async Task<GenericResponseDto<string>> ToggleActivationAsync()
    {
        var userId = GetCurrentUserId();
        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId, includes: new[] { "SellerOrderProducts", "BuyerOrderProducts" });
        if (user == null)
            throw new NotFoundException("User not found.");

        // If deactivating (IsDeleted = true), block if user has active orders or service requests
        if (!user.IsDeleted) // going to deactivate
        {
            // Check OrderProducts (as seller or buyer)
            bool hasActiveOrders = (user.SellerOrderProducts?.Any(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled) == true)
                || (user.BuyerOrderProducts?.Any(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled) == true);

            // Check ServiceRequests (as buyer)
            bool hasActiveServiceRequests = await _repo.ServiceRequestRepo.AnyAsync(r => r.BuyerId == user.Id && r.ServiceStatus != ServiceStatus.Delivered && r.ServiceStatus != ServiceStatus.Cancelled);

            if (hasActiveOrders || hasActiveServiceRequests)
                throw new BadRequestException("Cannot deactivate account with active orders or service requests. Please complete or cancel them first.");
        }

        user.IsDeleted = !user.IsDeleted;
        _repo.UserRepo.Update(user);
        await _repo.CompleteAsync();

        return new GenericResponseDto<string>
        {
            Message = user.IsDeleted
                ? "User deactivated successfully."
                : "User reactivated successfully.",
            Status = StatusCodes.Status200OK,
            Data = user.Id
        };
    }

    // Admin-specific methods
    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<UserReadDto>>>> GetAllUsersAsync(int page = 1, int size = 10, string searchTerm = null, bool? isActive = null, bool? isVerified = null, bool? isBlocked = null)
    {
        int skip = (page - 1) * size;

        // Build filter expression
        Expression<Func<User, bool>> filter = u => true;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            Expression<Func<User, bool>> searchFilter = u =>
                u.FirstName.ToLower().Contains(lowerTerm) ||
                u.LastName.ToLower().Contains(lowerTerm) ||
                u.Email.ToLower().Contains(lowerTerm) ||
                u.UserName.ToLower().Contains(lowerTerm);
            filter = filter.And(searchFilter);
        }

        if (isActive.HasValue)
        {
            var activeFilter = (Expression<Func<User, bool>>)(u => u.IsDeleted == !isActive.Value);
            filter = filter.And(activeFilter);
        }

        if (isVerified.HasValue)
        {
            if (isVerified.Value)
            {
                var verifiedFilter = (Expression<Func<User, bool>>)(u => u.UserVerification != null && u.UserVerification.Status == VerificationStatus.Approved);
                filter = filter.And(verifiedFilter);
            }
            else
            {
                var unverifiedFilter = (Expression<Func<User, bool>>)(u => u.UserVerification == null || u.UserVerification.Status != VerificationStatus.Approved);
                filter = filter.And(unverifiedFilter);
            }
        }

        // Blocked filter
        if (isBlocked.HasValue)
        {
            if (isBlocked.Value)
            {
                // User has an active ban (temporary or permanent)
                filter = filter.And(u => u.Punishments.Any(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) && (p.EndDate == null || p.EndDate > DateTime.UtcNow)));
            }
            else
            {
                // User does NOT have an active ban
                filter = filter.And(u => !u.Punishments.Any(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) && (p.EndDate == null || p.EndDate > DateTime.UtcNow)));
            }
        }

        var users = await _repo.UserRepo.FindAllAsync(
            filter,
            skip: skip,
            take: size,
            includes: ["UserVerification", "Punishments"]
        );

        int totalCount = await _repo.UserRepo.CountAsync(filter);

        var userDtos = new List<UserReadDto>();
        foreach (var user in users)
        {
            var dto = _mapper.Map<UserReadDto>(user);
            dto.IsApproved = user.IsApproved;
            dto.IsDeleted = user.IsDeleted;

            var roles = await _userManager.GetRolesAsync(user);
            dto.Roles = roles.ToList();

            // Add punishment info
            var currentPunishment = user.Punishments?
                .Where(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                           (p.EndDate == null || p.EndDate > DateTime.UtcNow))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            if (currentPunishment != null)
            {
                dto.CurrentPunishment = new PunishmentInfoDto
                {
                    Type = currentPunishment.Type.ToString(),
                    Reason = currentPunishment.Reason,
                    EndDate = currentPunishment.EndDate
                };
            }

            userDtos.Add(dto);
        }

        var result = new PaginatedDto<IEnumerable<UserReadDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = userDtos
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<UserReadDto>>>
        {
            Message = "Users retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = result
        };
    }

    public async Task<GenericResponseDto<string>> DeactivateUserAsync(string userId)
    {
        var user = await _repo.UserRepo.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.IsDeleted)
            throw new BadRequestException("User is already deactivated.");

        // Check OrderProducts (as seller or buyer) using direct query
        bool hasActiveOrders = await _repo.OrderRepo.AnyAsync(
            o => (o.SellerId == user.Id || o.BuyerId == user.Id) &&
                 o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled
        );

        // Check ServiceRequests (as buyer)
        bool hasActiveServiceRequests = await _repo.ServiceRequestRepo.AnyAsync(r => r.BuyerId == user.Id && r.ServiceStatus != ServiceStatus.Delivered && r.ServiceStatus != ServiceStatus.Cancelled);

        if (hasActiveOrders || hasActiveServiceRequests)
            throw new BadRequestException("Cannot deactivate account with active orders or service requests. Please complete or cancel them first.");

        user.IsDeleted = true;
        _repo.UserRepo.Update(user);
        await _repo.CompleteAsync();

        // Send email notification
        var emailBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #dc3545;'>Account Deactivated</h2>
            <p>Dear {user.FirstName},</p>
            <p>Your account has been deactivated by an administrator.</p>
            <p>If you believe this was done in error, please contact our support team.</p>
            <p>Best regards,<br>Stockat Support</p>
        </div>";

        await _emailService.SendEmailAsync(user.Email, "Account Deactivated - Stockat", emailBody);

        return new GenericResponseDto<string>
        {
            Message = "User deactivated successfully.",
            Status = StatusCodes.Status200OK,
            Data = userId
        };
    }

    public async Task<GenericResponseDto<string>> ActivateUserAsync(string userId)
    {
        var user = await _repo.UserRepo.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (!user.IsDeleted)
            throw new BadRequestException("User is already active.");

        // Allow activation at any time (no checks)
        user.IsDeleted = false;
        _repo.UserRepo.Update(user);
        await _repo.CompleteAsync();

        // Send email notification
        var emailBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #28a745;'>Account Reactivated</h2>
            <p>Dear {user.FirstName},</p>
            <p>Your account has been reactivated by an administrator.</p>
            <p>You can now access your account normally.</p>
            <p>Best regards,<br>Stockat Support</p>
        </div>";

        await _emailService.SendEmailAsync(user.Email, "Account Reactivated - Stockat", emailBody);

        return new GenericResponseDto<string>
        {
            Message = "User activated successfully.",
            Status = StatusCodes.Status200OK,
            Data = userId
        };
    }

    public async Task<GenericResponseDto<UserReadDto>> GetUserWithDetailsAsync(string userId)
    {
        var includes = new[] { "UserVerification", "Punishments", "Products", "Services", "CreatedAuctions" };

        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId, includes);
        if (user == null)
            throw new NotFoundException("User not found.");

        var dto = _mapper.Map<UserReadDto>(user);
        dto.IsApproved = user.IsApproved;
        dto.IsDeleted = user.IsDeleted;

        var roles = await _userManager.GetRolesAsync(user);
        dto.Roles = roles.ToList();

        // Add detailed punishment info
        var allPunishments = user.Punishments?.OrderByDescending(p => p.CreatedAt).ToList() ?? new List<UserPunishment>();
        dto.PunishmentHistory = allPunishments.Select(p => new PunishmentHistoryDto
        {
            Type = p.Type.ToString(),
            Reason = p.Reason,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                      (p.EndDate == null || p.EndDate > DateTime.UtcNow)
        }).ToList();

        // Add statistics
        dto.Statistics = new UserStatisticsDto
        {
            TotalProducts = user.Products?.Count ?? 0,
            TotalServices = user.Services?.Count ?? 0,
            TotalAuctions = user.CreatedAuctions?.Count ?? 0,
            TotalPunishments = allPunishments.Count,
            ActivePunishments = allPunishments.Count(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                                                         (p.EndDate == null || p.EndDate > DateTime.UtcNow))
        };

        return new GenericResponseDto<UserReadDto>
        {
            Message = "User details retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = dto
        };
    }

    // Helper
    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return userId;
    }

    public async Task<GenericResponseDto<object>> GetUserStatisticsAsync()
    {
        // Total users
        var total = await _repo.UserRepo.CountAsync(u => true);
        // Active users (not deleted)
        var active = await _repo.UserRepo.CountAsync(u => !u.IsDeleted);
        // Inactive users (deleted)
        var inactive = await _repo.UserRepo.CountAsync(u => u.IsDeleted);
        // Verified users
        var verified = await _repo.UserRepo.CountAsync(u => u.UserVerification != null && u.UserVerification.Status == VerificationStatus.Approved);
        // Unverified users
        var unverified = await _repo.UserRepo.CountAsync(u => u.UserVerification != null && u.UserVerification.Status == VerificationStatus.Pending);
        // Blocked users (active ban)
        var blocked = await _repo.UserRepo.CountAsync(u => u.Punishments.Any(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) && (p.EndDate == null || p.EndDate > DateTime.UtcNow)));

        var stats = new
        {
            total,
            active,
            inactive,
            verified,
            unverified,
            blocked
        };

        return new GenericResponseDto<object>
        {
            Message = "User statistics retrieved successfully.",
            Status = StatusCodes.Status200OK,
            Data = stats
        };
    }

    public async Task<GenericResponseDto<string>> UpgradeToSellerAsync()
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Seller"))
        {
            return new GenericResponseDto<string>
            {
                Message = "User is already a seller.",
                Status = StatusCodes.Status400BadRequest,
                Data = userId
            };
        }

        var result = await _userManager.AddToRoleAsync(user, "Seller");
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return new GenericResponseDto<string>
            {
                Message = "Failed to upgrade to seller: " + errors,
                Status = StatusCodes.Status400BadRequest,
                Data = userId
            };
        }

        return new GenericResponseDto<string>
        {
            Message = "User upgraded to seller successfully.",
            Status = StatusCodes.Status200OK,
            Data = userId
        };
    }


    // GetCurrentUserID
    public async Task<string> GetCurrentUserIdAsyncService()
    {

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return userId;

    }
}
