﻿using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
