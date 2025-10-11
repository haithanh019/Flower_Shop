using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // Thêm using này

namespace Ultitity.Exceptions
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger; // Thêm ILogger

        public ValidationExceptionMiddleware(
            RequestDelegate next,
            ILogger<ValidationExceptionMiddleware> logger
        ) // Cập nhật constructor
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (CustomValidationException ex)
            {
                await HandleValidationExceptionAsync(httpContext, ex);
            }
            catch (Exception ex) // Bắt tất cả các loại exception khác
            {
                // Ghi lại toàn bộ chi tiết của lỗi
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(httpContext);
            }
        }

        private static async Task HandleValidationExceptionAsync(
            HttpContext context,
            CustomValidationException exception
        )
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new { message = exception.Message, errors = exception.Errors };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }

        private static async Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // Giữ thông báo lỗi chung chung cho client, nhưng lỗi chi tiết đã được log ở server
            var response = new
            {
                message = "An unexpected error occurred. Please check the API logs for details.",
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
