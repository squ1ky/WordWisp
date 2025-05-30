﻿using WordWisp.API.Models.DTOs.Auth;

namespace WordWisp.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
        Task<bool> ResendVerificationCodeAsync(string email);
    }
}
