using BusinessLogic.DTOs.Auth;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto registerRequest);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task RequestPasswordResetAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequest);
    }
}
