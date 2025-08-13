using EcoFashionBackEnd.Data;
using EcoFashionBackEnd.Data.test;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Extensions;
using EcoFashionBackEnd.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

public class Program
{
    public static async Task Main(string[] args)
    {
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddInfrastructure(builder.Configuration);

            // Add services to the container.
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


            builder.Services.AddCors(option =>
            option.AddPolicy("CORS", builder =>
                builder.WithOrigins("http://localhost:5173", "http://localhost:5174")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()));



            // Add JSON options to handle potential circular references
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                // Sử dụng camelCase cho JSON API để tương thích với frontend JavaScript/TypeScript
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

            var app = builder.Build();

            // Database migration and seeding
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

            // Configure the HTTP request pipeline.
            
            // Enable Swagger for all environments
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });
            
            // Database migration only in Development
            if (app.Environment.IsDevelopment())
            {
                await using (var scope = app.Services.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await dbContext.Database.MigrateAsync();
                }
            }


            app.UseHttpsRedirection();
            app.UseCors("CORS");
            app.UseMiddleware<ExceptionMiddleware>();


            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}