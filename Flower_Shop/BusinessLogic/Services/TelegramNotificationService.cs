using BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services
{
    public class TelegramNotificationService : INotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramNotificationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
        )
        {
            _httpClientFactory = httpClientFactory;
            _botToken = configuration["Telegram:BotToken"] ?? string.Empty;
            _chatId = configuration["Telegram:ChatId"] ?? string.Empty;
        }

        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(_chatId))
            {
                return;
            }

            var url =
                $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(message)}&parse_mode=Markdown";
            var client = _httpClientFactory.CreateClient();

            try
            {
                await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Telegram message: {ex.Message}");
            }
        }
    }
}
