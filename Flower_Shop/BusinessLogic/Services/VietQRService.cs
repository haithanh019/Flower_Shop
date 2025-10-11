using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services
{
    public class VietQRService : IVietQRService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VietQRService> _logger;

        public VietQRService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<VietQRService> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> GenerateQRCode(Order order)
        {
            try
            {
                var bankBinString = _configuration["VietQR:BankBin"];
                var accountNumber = _configuration["VietQR:AccountNumber"];
                var accountName = _configuration["VietQR:AccountName"];
                var clientId = _configuration["VietQR:ClientId"];
                var apiKey = _configuration["VietQR:ApiKey"];
                var apiUrl = _configuration["VietQR:ApiUrl"] + "generate";

                if (
                    string.IsNullOrEmpty(bankBinString)
                    || string.IsNullOrEmpty(accountNumber)
                    || string.IsNullOrEmpty(accountName)
                    || string.IsNullOrEmpty(clientId)
                    || string.IsNullOrEmpty(apiKey)
                )
                {
                    _logger.LogError("VietQR configuration is missing in appsettings.json.");
                    throw new InvalidOperationException(
                        "VietQR configuration is not set correctly."
                    );
                }

                if (!int.TryParse(bankBinString, out var bankBin))
                {
                    _logger.LogError("Invalid BankBin format in VietQR configuration.");
                    throw new InvalidOperationException("VietQR BankBin must be a valid number.");
                }

                var payload = new
                {
                    acqId = bankBin,
                    accountNo = accountNumber,
                    accountName = accountName,
                    amount = (int)order.TotalAmount,
                    addInfo = order.OrderNumber,
                    format = "qr_only",
                    template = "compact2",
                };

                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("x-client-id", clientId);
                request.Headers.Add("x-api-key", apiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.SendAsync(request);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(jsonString);

                    // --- BẮT ĐẦU SỬA LỖI ---
                    // Kiểm tra an toàn sự tồn tại của thuộc tính "data"
                    if (!doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        _logger.LogWarning(
                            "VietQR response does not contain 'data' property. Response: {JsonResponse}",
                            jsonString
                        );
                        return null;
                    }

                    // Ưu tiên lấy qrDataURL (ảnh base64)
                    if (dataElement.TryGetProperty("qrDataURL", out var qrDataUrlElement))
                    {
                        return qrDataUrlElement.GetString();
                    }

                    // Nếu không có, dự phòng bằng cách tự tạo URL ảnh từ qrContent
                    if (dataElement.TryGetProperty("qrContent", out var qrContentElement))
                    {
                        var qrContent = qrContentElement.GetString();
                        var imageUrl =
                            $"https://api.vietqr.io/image/{bankBinString}-{accountNumber}-compact2.png?amount={(int)order.TotalAmount}&addInfo={order.OrderNumber}&accountName={accountName}";
                        _logger.LogInformation(
                            "Generated VietQR image URL from qrContent: {ImageUrl}",
                            imageUrl
                        );
                        return imageUrl;
                    }

                    _logger.LogWarning(
                        "VietQR 'data' property does not contain 'qrDataURL' or 'qrContent'. Response: {JsonResponse}",
                        jsonString
                    );
                    return null;
                    // --- KẾT THÚC SỬA LỖI ---
                }
                else
                {
                    _logger.LogError(
                        "Failed to generate VietQR code. Status: {StatusCode}, Response: {Error}",
                        response.StatusCode,
                        jsonString
                    );
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An exception occurred while generating VietQR code for Order #{OrderNumber}",
                    order.OrderNumber
                );
                return null;
            }
        }
    }
}
