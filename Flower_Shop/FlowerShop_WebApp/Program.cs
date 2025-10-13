using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;

namespace FlowerShop_WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("vi-VN") };
                options.DefaultRequestCulture = new RequestCulture("vi-VN");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            builder.Services.AddHttpClient(
                "ApiClient",
                client =>
                {
                    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        throw new InvalidOperationException(
                            "ApiSettings:BaseUrl is not configured"
                        );
                    }
                    client.BaseAddress = new Uri(baseUrl!);
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                }
            );
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder
                .Services.AddAuthentication("CookieAuth")
                .AddCookie(
                    "CookieAuth",
                    options =>
                    {
                        options.Cookie.Name = "FlowerShop.Auth";
                        options.LoginPath = "/Account/Login";
                        options.AccessDeniedPath = "/Home/AccessDenied";
                    }
                );
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRequestLocalization();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "AdminArea",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
