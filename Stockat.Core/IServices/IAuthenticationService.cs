﻿using Microsoft.AspNetCore.Identity;
using Stockat.Core.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IAuthenticationService
{
    Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration);
    Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);
    Task<TokenDto> CreateToken(bool populateExp);
    Task<TokenDto> RefreshToken(TokenDto tokenDto);

    Task<TokenDto> ExternalLoginAsync(ExternalAuthDto externalAuth);

    Task ConfirmEmail(string userId, string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPassword);

    Task LogoutAsync(string username);

}