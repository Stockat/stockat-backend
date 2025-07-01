using System;

namespace Stockat.Core.DTOs.ChatDTOs;

/// <summary>
/// Minimal user info for chat display (id, name, image).
/// </summary>
public class UserChatInfoDto
{
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string ProfileImageUrl { get; set; }
}