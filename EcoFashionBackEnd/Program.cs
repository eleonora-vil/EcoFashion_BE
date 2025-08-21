using EcoFashionBackEnd.Data;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Extensions;
using EcoFashionBackEnd.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Add custom services (DbContext, Auth, Mail, Cloudinary, Vnpay, Repository, etc.)
        builder.Services.AddInfrastructure(builder.Configuration);

        // 2. Heroku / Railway PORT config
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://*:{port}");

        // 3. Health check
        builder.Services.AddHealthChecks();

        // 4. Swagger config
        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "BE API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        // 5. CORS config
        builder.Services.AddCors(option =>
            option.AddPolicy("CORS", policy =>
                policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()));

        // 6. JSON config
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        var app = builder.Build();

        // 7. Health check endpoint
        app.UseHealthChecks("/health");

        // 8. Database migration + seeding (chỉ chạy 1 lần duy nhất khi start app)
        try
        {
            Console.WriteLine("Starting database initialization...");
            await app.InitialiseDatabaseAsync();
            Console.WriteLine("Database initialization completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        // 9. Swagger
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        // 10. Middlewares
        app.UseCors("CORS");
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        // 11. Controllers
        app.MapControllers();

        app.Run();
    }
}
