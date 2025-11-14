using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OHairGanic.BLL.Implementations;
using OHairGanic.BLL.Integrations;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Implementations;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

var cs = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<OHairGanicDBContext>(opt =>
    opt.UseSqlServer(cs, sql =>
    {
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        sql.CommandTimeout(30);
    }));


// ================== CONFIG: CLOUDINARY ==================
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Đăng ký Cloudinary client
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new Cloudinary(new Account(
        settings.CloudName,
        settings.ApiKey,
        settings.ApiSecret
    ));
});

// Đăng ký CloudinaryService (upload, delete)
builder.Services.AddScoped<CloudinaryService>();

// ================== CONFIG: PAYOS ==================
builder.Services.Configure<PayOSSettings>(
    builder.Configuration.GetSection("PayOS"));
builder.Services.AddHttpClient(); // cần thiết để call API PayOS

// ================== CORS ==================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // ✅ Cho phép tất cả origin
            .AllowAnyHeader()      // ✅ Cho phép tất cả header
            .AllowAnyMethod();     // ✅ Cho phép GET, POST, PUT, DELETE, PATCH, OPTIONS
    });
});

// ================== JWT AUTHENTICATION ==================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Lấy config trực tiếp từ appsettings (không BuildServiceProvider)
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        NameClaimType = "nameid",
        RoleClaimType = "role"
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT ERROR: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("⚠️ JWT Challenge Triggered: Token missing or invalid");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ JWT VALIDATED successfully");
            return Task.CompletedTask;
        }
    };
});

// ================== SWAGGER ==================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OHairGanic API",
        Version = "v1",
        Description = "API documentation for OHairGanic"
    });

    // Thêm JWT Bearer Auth vào Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào đây (chỉ phần token, không cần 'Bearer ')",

        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    opt.AddSecurityDefinition("Bearer", securityScheme);

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ================== DEPENDENCY INJECTION ==================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repository layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICaptureRepository, CaptureRepository>();
builder.Services.AddScoped<IAnalyzeRepository, AnalyzeRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Service layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICaptureService, CaptureService>();
builder.Services.AddScoped<IAnalyzeService, AnalyzeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();


builder.Services.AddScoped<IPaymentGateway, PayOSGateway>();

builder.Services.AddSingleton<IHairAnalysisService, OnnxHairService>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ================== APP PIPELINE ==================
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OHairGanicDBContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OHairGanic API v1");
    });
}

// Middleware order quan trọng
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
