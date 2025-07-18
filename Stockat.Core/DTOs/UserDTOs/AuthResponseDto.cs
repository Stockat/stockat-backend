﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.UserDTOs;

public class AuthResponseDto
{
    public TokenDto Token { get; set; }
    public bool IsAuthSuccessful { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsApproved { get; set; }
    public string? Message { get; set; } // optional
}