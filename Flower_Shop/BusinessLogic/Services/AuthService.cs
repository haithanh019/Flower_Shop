using AutoMapper;
using BusinessLogic.DTOs.Auth;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Ultitity.Email.Interface;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IEmailQueue _emailQueue;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITokenService tokenService,
            IEmailQueue emailQueue
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailQueue = emailQueue;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Email == loginRequest.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Credentials", new[] { "Invalid email or password." } },
                    }
                );
            }

            var (token, expiresAt) = _tokenService.CreateToken(user);

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = token,
                ExpiresAt = expiresAt,
            };
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
        {
            var emailExists = await _unitOfWork.User.GetAsync(u =>
                u.Email == registerRequest.Email
            );
            if (emailExists != null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email is already taken." } },
                    }
                );
            }

            var newUser = _mapper.Map<User>(registerRequest);
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            await _unitOfWork.User.AddAsync(newUser);
            await _unitOfWork.Cart.AddAsync(new Cart { UserId = newUser.UserId });

            await _unitOfWork.SaveAsync();

            var (token, expiresAt) = _tokenService.CreateToken(newUser);

            return new LoginResponseDto
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                Role = newUser.Role.ToString(),
                AccessToken = token,
                ExpiresAt = expiresAt,
            };
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequestDto request)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return;
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            user.PasswordResetToken = BCrypt.Net.BCrypt.HashPassword(otp);

            user.ResetTokenExpiry = expiryTime;

            await _unitOfWork.SaveAsync();

            var subject = "[FlowerShop] Yêu cầu đặt lại mật khẩu";
            var htmlMessage = EmailTemplateService.PasswordResetOtpEmail(user, otp);

            _emailQueue.QueueEmail(user.Email, subject, htmlMessage);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Email == request.Email);

            if (
                user == null
                || string.IsNullOrEmpty(user.PasswordResetToken)
                || user.ResetTokenExpiry == null
                || user.ResetTokenExpiry < DateTime.UtcNow
            )
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Otp", new[] { "Mã OTP không hợp lệ hoặc đã hết hạn." } },
                    }
                );
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Otp, user.PasswordResetToken))
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Otp", new[] { "Mã OTP không chính xác." } },
                    }
                );
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;

            await _unitOfWork.SaveAsync();
        }
    }
}
