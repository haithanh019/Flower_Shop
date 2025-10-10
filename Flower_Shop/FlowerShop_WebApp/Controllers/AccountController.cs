using System.IdentityModel.Tokens.Jwt;
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
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            ILogger<AccountController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync("api/auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseViewModel>(
                    jsonString,
                    _jsonOptions
                );

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    _logger.LogWarning(
                        "[LOGIN SUCCESS] Token received: {Token}",
                        loginResponse.AccessToken
                    );
                    HttpContext.Session.SetString("JWToken", loginResponse.AccessToken);

                    var claimsPrincipal = CreateClaimsPrincipalFromToken(loginResponse.AccessToken);
                    await HttpContext.SignInAsync(
                        "CookieAuth",
                        claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = true }
                    );

                    return RedirectToLocal(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Các action Register, Logout, và helper methods khác
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
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
            var response = await client.PostAsync("api/auth/register", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var registerResponse = JsonSerializer.Deserialize<LoginResponseViewModel>(
                    jsonString,
                    _jsonOptions
                );
                if (registerResponse != null && !string.IsNullOrEmpty(registerResponse.AccessToken))
                {
                    HttpContext.Session.SetString("JWToken", registerResponse.AccessToken);
                    var claimsPrincipal = CreateClaimsPrincipalFromToken(
                        registerResponse.AccessToken
                    );
                    await HttpContext.SignInAsync(
                        "CookieAuth",
                        claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = true }
                    );
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Registration failed.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // PHỤC HỒI PHƯƠNG THỨC ĐỌC CLAIM ĐÚNG
        private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value ?? ""
                ),
                new(
                    ClaimTypes.Email,
                    jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? ""
                ),
                new(
                    ClaimTypes.Role,
                    jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? ""
                ),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(claimsIdentity);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
