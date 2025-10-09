using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Action hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Action xử lý khi người dùng nhấn nút "Login"
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            // 1. Gửi yêu cầu đăng nhập đến API
            var response = await client.PostAsync("api/auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseViewModel>(
                    jsonString,
                    _jsonOptions
                );

                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Login failed: Invalid response from server."
                    );
                    return View(model);
                }

                // 2. Lưu JWT Token vào Session để dùng cho các lời gọi API sau này
                HttpContext.Session.SetString("JWToken", loginResponse.AccessToken);

                // 3. Đọc thông tin từ JWT và tạo Cookie xác thực cho WebApp
                var claimsPrincipal = CreateClaimsPrincipalFromToken(loginResponse.AccessToken);
                await HttpContext.SignInAsync(
                    "CookieAuth",
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true, // Ghi nhớ đăng nhập
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                    }
                );

                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Nếu API trả về lỗi (sai mật khẩu, email không tồn tại)
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
        }

        // Action hiển thị form đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Action xử lý khi người dùng nhấn nút "Register"
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var requestBody = new
            {
                model.Email,
                model.Password,
                model.FullName,
                model.PhoneNumber,
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // 1. Gửi yêu cầu đăng ký đến API
            var response = await client.PostAsync("api/auth/register", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // 2. Nếu đăng ký thành công, API sẽ trả về token, tiến hành đăng nhập luôn
                var jsonString = await response.Content.ReadAsStringAsync();
                var registerResponse = JsonSerializer.Deserialize<LoginResponseViewModel>(
                    jsonString,
                    _jsonOptions
                );

                if (registerResponse == null || string.IsNullOrEmpty(registerResponse.AccessToken))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Registration failed: Invalid response from server."
                    );
                    return View(model);
                }

                // Lưu JWT và tạo cookie (giống hệt logic của Login)
                HttpContext.Session.SetString("JWToken", registerResponse.AccessToken);
                var claimsPrincipal = CreateClaimsPrincipalFromToken(registerResponse.AccessToken);
                await HttpContext.SignInAsync(
                    "CookieAuth",
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                    }
                );

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // 3. Nếu API trả về lỗi (ví dụ: email đã tồn tại)
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. The email might already be in use."
                );
                return View(model);
            }
        }

        // Action Đăng xuất
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // Helper method để tạo ClaimsPrincipal từ JWT
        private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = new List<Claim>
            {
                // Lấy các claim cần thiết từ token để định danh người dùng trong WebApp
                new(
                    ClaimTypes.NameIdentifier,
                    jwtToken
                        .Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)
                        ?.Value ?? ""
                ),
                new(
                    ClaimTypes.Email,
                    jwtToken
                        .Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)
                        ?.Value ?? ""
                ),
                new(
                    ClaimTypes.Role,
                    jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? ""
                ),
                // Thêm các claim khác nếu cần
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(claimsIdentity);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
