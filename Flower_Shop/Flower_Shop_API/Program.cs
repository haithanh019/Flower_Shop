using System.Text;
using BusinessLogic.Mapping;
using BusinessLogic.Services;
using BusinessLogic.Services.FacadeService;
using BusinessLogic.Services.Interfaces;
using DataAccess.Data;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ultitity.Clients.Groqs;
using Ultitity.Email;
using Ultitity.Email.Interface;
using Ultitity.Exceptions;
using Ultitity.Options;

namespace Flower_Shop_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<FlowerShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            // Add services to the container.
            var key = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT:Key is missing in configuration.");
            }
            builder
                .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    };
                });
            builder
                .Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Cấu hình này sẽ bảo API luôn xử lý tên thuộc tính JSON
                    // dưới dạng camelCase, ví dụ: "productId" thay vì "ProductId".
                    options.JsonSerializerOptions.PropertyNamingPolicy = System
                        .Text
                        .Json
                        .JsonNamingPolicy
                        .CamelCase;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            /// Register Options
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<CloudinaryOptions>(
                builder.Configuration.GetSection("Cloudinary")
            );
            builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));
            builder.Services.AddLogging();
            /// Register services
            builder.Services.AddScoped<CoreDependencies>();
            builder.Services.AddScoped<InfraDependencies>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IFacadeService, FacadeService>();
            builder.Services.AddSingleton<EmailSender>();
            builder.Services.AddHostedService<BackgroundEmailSender>();
            builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
            builder.Services.AddScoped<IPayOSService, PayOSService>();

            // LLM client
            builder.Services.AddHttpClient<IGroqClient, GroqClient>(client =>
            {
                var baseUrl =
                    builder.Configuration["Groq:BaseUrl"]
                    ?? throw new InvalidOperationException("Missing Groq:BaseUrl");
                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowSpecificOrigins",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                "https://unarriving-unswaying-winifred.ngrok-free.dev",
                                "https://flowershopwebapp.azurewebsites.net",
                                "https://myhongshop.com"
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ValidationExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowSpecificOrigins");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
