namespace FlowerShop_WebApp.Models.Auth
{
    public class LoginResponseViewModel
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
