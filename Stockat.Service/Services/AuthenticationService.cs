﻿using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Stockat.Core;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Stockat.Service.Services;
internal sealed class AuthenticationService : IAuthenticationService
{
    // it would be internal since we will never use this class outside this class library 
    // api layer will only deal with the service manager (unit of work)
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IChatService _chatService;
    private readonly IRepositoryManager _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User? _user;
    public AuthenticationService(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IEmailService emailService, IChatService chatService, IRepositoryManager repo, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _chatService = chatService;
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthDto externalAuth)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
            return payload;
        }
        catch (Exception ex)
        {
            //log an exception
            return null;
        }
    }

    // google external loging service
    public async Task<AuthenticationStatus> ExternalLoginAsync(ExternalAuthDto externalAuth)
    {
        var payload = await VerifyGoogleToken(externalAuth);
        if (payload == null)
            return AuthenticationStatus.InvalidCredentials;

        var loginInfo = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);
        var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    UserName = payload.Email.Split('@')[0],
                    EmailConfirmed = true,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? ""
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return AuthenticationStatus.InvalidCredentials;

                await _userManager.AddToRoleAsync(user, "Buyer");
                await _userManager.AddLoginAsync(user, loginInfo);

                var admin = await _userManager.FindByEmailAsync(_configuration["Admin:Email"]);
                var createdUser = await _userManager.FindByEmailAsync(payload.Email);
                var newConversationWithAdmin = await _chatService.CreateConversationAsync(admin.Id, createdUser.Id);
                await _repo.CompleteAsync();

                var welcomeMessage = await _chatService.SendMessageAsync(new Core.DTOs.ChatDTOs.SendMessageDto()
                {
                    ConversationId = newConversationWithAdmin.ConversationId,
                    MessageText = $"Welcome to Stockat, {user.FirstName} {user.LastName}. We're excited to have you. Let us know if you need anything — we're here to help!"

                },
                    admin.Id
                );

                await _repo.CompleteAsync();
            }
            else
            {
                await _userManager.AddLoginAsync(user, loginInfo);
            }
        }

        // Always reload user with punishments and verification
        _user = await _repo.UserRepo.FindAsync(
            u => u.Id == user.Id,
            new[] { "Punishments", "UserVerification" }
        );

        if (await _userManager.IsLockedOutAsync(_user))
            return AuthenticationStatus.InvalidCredentials;

        if (_user.IsDeleted)
        {
            _logger.LogWarn("ExternalLoginAsync: Account is soft-deleted.");
            return AuthenticationStatus.AccountDeleted;
        }

        if (_user.IsBlocked)
        {
            _logger.LogWarn("ExternalLoginAsync: Account is blocked.");
            return AuthenticationStatus.Blocked;
        }

        return AuthenticationStatus.Success;
    }


    // register
    public async Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration)
    {
        var user = _mapper.Map<User>(userForRegistration);
        user.EmailConfirmed = false;
        user.UserName = userForRegistration.Email.Split('@')[0]; // updated here

        var result = await _userManager.CreateAsync(user, userForRegistration.Password);


        if (result.Succeeded && await _roleManager.RoleExistsAsync("Buyer"))
        {
            await _userManager.AddToRoleAsync(user, "Buyer");

            await SendConfirmationEmail(user);
        }

        return result;
    }

    // login
    public async Task<AuthenticationStatus> ValidateUser(UserForAuthenticationDto userForAuth)
    {
        _user = await _repo.UserRepo.FindAsync(
            u => u.Email == userForAuth.Email,
            new[] { "Punishments", "UserVerification" }
        );

        var isValidUser = _user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password);

        if (!isValidUser)
        {
            _logger.LogWarn($"{nameof(ValidateUser)}: Invalid credentials.");
            return AuthenticationStatus.InvalidCredentials;
        }

        if (!_user.EmailConfirmed)
        {
            _logger.LogWarn($"{nameof(ValidateUser)}: Email not confirmed.");
            await SendConfirmationEmail(_user);
            return AuthenticationStatus.EmailNotConfirmed;
        }

        if (_user.IsDeleted)
        {
            _logger.LogWarn($"{nameof(ValidateUser)}: Account is soft-deleted.");
            return AuthenticationStatus.AccountDeleted;
        }

        if (_user.IsBlocked)
        {
            _logger.LogWarn($"{nameof(ValidateUser)}: Account is blocked.");
            return AuthenticationStatus.Blocked;
        }

        return AuthenticationStatus.Success;
    }



    //
    public async Task ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found"); // custom exception

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            throw new BadRequestException("Email confirmation failed");

        var admin = await _userManager.FindByEmailAsync(_configuration["Admin:Email"]);
        var newConversationWithAdmin = await _chatService.CreateConversationAsync(admin.Id, user.Id);
        await _repo.CompleteAsync();

        var welcomeMessage = await _chatService.SendMessageAsync(new Core.DTOs.ChatDTOs.SendMessageDto()
            {
                ConversationId = newConversationWithAdmin.ConversationId,
                MessageText = $"Welcome to Stockat, {user.FirstName} {user.LastName}. We're excited to have you. Let us know if you need anything — we're here to help!"

            },
            admin.Id
        );

        await _repo.CompleteAsync();
    }

    public async Task LogoutAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
            throw new NotFoundException("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = _configuration["Frontend:BaseUrl"];
        var resetLink = $"{frontendUrl}/reset-password?email={email}&token={WebUtility.UrlEncode(token)}";

        var message = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 8px; padding: 24px;'>
  <h2 style='color: #db2777; text-align: center;'>Reset Your Password</h2>
  <p style='font-size: 1.1em;'>Hi {user.FirstName},</p>
  <p>We received a request to reset your password. Click the button below to set a new password:</p>
  <div style='text-align: center; margin: 24px 0;'>
    <a href='{resetLink}' style='background: #db2777; color: #fff; padding: 12px 24px; border-radius: 4px; text-decoration: none; font-weight: bold;'>Reset Password</a>
  </div>
  <p>If you did not request a password reset, you can safely ignore this email.</p>
  <hr style='margin: 32px 0; border: none; border-top: 1px solid #eee;'/>
  <p style='font-size: 0.9em; color: #888; text-align: center;'>Stockat &copy; {DateTime.Now.Year} &mdash; <a href='https://stockat.com' style='color: #db2777;'>Visit our website</a></p>
</div>";
        await _emailService.SendEmailAsync(user.Email, "Reset your password - Stockat", message);
    }

    // will be called to serve the forgot password endpoint
    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new NotFoundException("User not found");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new BadRequestException("Password reset failed");
    }

    public async Task<TokenDto> CreateToken(bool populateExp, string? userId = null)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims(userId);
        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

        var refreshToken = GenerateRefreshToken();
        _user.RefreshToken = refreshToken;
        if (populateExp)
            _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        await _userManager.UpdateAsync(_user);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return new TokenDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
        var user = await _userManager.FindByNameAsync(principal.Identity.Name);
        if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            throw new BadRequestException("Invalid client request. The tokenDto has some invalid values.");
        _user = user;
        return await CreateToken(populateExp: false);
    }

    // helpers
    private SigningCredentials GetSigningCredentials()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["secretKey"];
        var key = Encoding.UTF8.GetBytes(secretKey);

        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    private async Task<List<Claim>> GetClaims(string? userId = null)
    {
        

        if(userId is not null)
        {
            _user = await _userManager.FindByIdAsync(userId);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _user.UserName),
            new Claim(ClaimTypes.NameIdentifier, _user.Id)
        };

        var roles = await _userManager.GetRolesAsync(_user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var tokenOptions = new JwtSecurityToken
        (
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
            signingCredentials: signingCredentials
        );

        return tokenOptions;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["secretKey"])),
            ValidateLifetime = false,
            ValidIssuer = jwtSettings["validIssuer"],
            ValidAudience = jwtSettings["validAudience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
        return principal;
    }

    private async Task SendConfirmationEmail(User user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = _configuration["Frontend:BaseUrl"];
        var confirmationLink = $"{frontendUrl}/confirm-email?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

        var emailMessage = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #f9f9f9; border-radius: 8px; padding: 24px;'>
  <h2 style='color: #db2777; text-align: center;'>Welcome to Stockat!</h2>
  <p style='font-size: 1.1em;'>Hi {user.FirstName},</p>
  <p>Thank you for registering with Stockat. Please confirm your email address to activate your account:</p>
  <div style='text-align: center; margin: 24px 0;'>
    <a href='{confirmationLink}' style='background: #db2777; color: #fff; padding: 12px 24px; border-radius: 4px; text-decoration: none; font-weight: bold;'>Confirm Email</a>
  </div>
  <p>If you did not create this account, please ignore this email or <a href='mailto:support@stockat.com'>contact support</a>.</p>
  <hr style='margin: 32px 0; border: none; border-top: 1px solid #eee;'/>
  <p style='font-size: 0.9em; color: #888; text-align: center;'>Stockat &copy; {DateTime.Now.Year} &mdash; <a href='https://stockat.com' style='color: #db2777;'>Visit our website</a></p>
</div>";
        await _emailService.SendEmailAsync(user.Email, "Confirm your email - Stockat", emailMessage);
    }

    public async Task<User> GetCurrentUser()
    {
        if (_user == null)
            throw new InvalidOperationException("No user is currently authenticated.");
        
        return _user;
    }

    public async Task<bool> GetCurrentUserApprovalStatus()
    {
        if (_user == null)
            throw new InvalidOperationException("No user is currently authenticated.");
        
        return _user.IsApproved;
    }
}
