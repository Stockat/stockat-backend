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
using System.Security.Claims;

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

    public async Task<GenericResponseDto<UserReadDto>> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();

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

        var user = await _repo.UserRepo.FindAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("User not found.");

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





    // Helper
    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return userId;
    }

}
