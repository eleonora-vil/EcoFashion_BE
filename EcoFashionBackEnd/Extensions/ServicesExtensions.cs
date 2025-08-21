using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Settings;
using AutoMapper;
using EcoFashionBackEnd.Helpers;
using EcoFashionBackEnd.Middlewares;
using EcoFashionBackEnd.Mapper;
using EcoFashionBackEnd.Helpers.Photos;
using EcoFashionBackEnd.Data.test;
using EcoFashionBackEnd.Extensions.NewFolder;


namespace EcoFashionBackEnd.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ExceptionMiddleware>();
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        //Add Mapper
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new ApplicationMapper());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        //Set time
        //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        services.Configure<JwtSettings>(val =>
        {
            val.Key = jwtSettings.Key;
        });

        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        services.Configure<CloudSettings>(configuration.GetSection(nameof(CloudSettings)));

        services.AddAuthorization();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("SqlDbConnection"));
        });

        services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IDatabaseInitialiser, DatabaseInitialiser>();

        services.AddScoped<UserService>();
        services.AddScoped<UserRoleService>();
        services.AddScoped<DesignerService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<ApplicationService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<CloudService>();
        services.AddScoped<DesignService>();
        // Material System Services (in dependency order)
        services.AddScoped<MaterialQueryService>();
        services.AddScoped<MaterialEnrichmentService>();
        services.AddScoped<MaterialBusinessService>();
        services.AddScoped<MaterialService>();
        services.AddScoped<MaterialTypeService>();
        services.AddScoped<DesignerMaterialInventoryService>();
        services.AddScoped<DesignDraftService>();
        services.AddScoped<DesignTypeService>();
        services.AddScoped<SustainabilityService>();
        services.AddScoped<BlogService>();
        services.AddScoped<OrderService>();
        services.AddScoped<CheckoutService>();
        services.AddScoped<OrderDetailService>();
        services.AddScoped<CartService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<MaterialInventoryService>();
        services.AddScoped<InventoryAnalyticsService>();
        //services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<PaymentService>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<ProductService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<DesignsVariantService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<InventoryTransactionService>();
        services.AddScoped<WalletService>();




        //background service 

        services.AddScoped<IOrderPayoutService, OrderPayoutService>();
        services.AddHostedService<OrderPayoutBackgroundService>();




        return services;
    }
}