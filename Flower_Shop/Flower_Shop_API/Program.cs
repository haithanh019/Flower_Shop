using System.Text;
using BusinessLogic.Services.FacadeService;
using DataAccess.Data;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ultitity.Clients.Groqs;
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    };
                });
            builder.Services.AddControllers();
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

            /// Register services
            builder.Services.AddScoped<CoreDependencies>();
            builder.Services.AddScoped<InfraDependencies>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IFacadeService, FacadeService>();

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

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
