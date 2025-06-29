using Microsoft.AspNetCore.Http;

namespace Stockat.Core.DTOs.UserDTOs;

public class UserImageUpdateDto
{
    public IFormFile ProfileImage { get; set; }
}
