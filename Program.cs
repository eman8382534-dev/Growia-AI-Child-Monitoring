using GrawiaaApp.API.Data;
using GrawiaaApp.API.Interfaces;
using GrawiaaApp.API.Logic;
using GrawiaaApp.API.Models;
using GrawiaaApp.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ================= Controllers =================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// ================= Database =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
// ================= JWT =================
// مهم: لازم AddAuthentication تيجي قبل AddIdentity
// لأن AddIdentity بتعمل override على DefaultAuthenticateScheme وترجعه لـ Cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
           Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        // مهم: لازم نحدد الـ claim اللي فيه الـ Role عشان [Authorize(Roles="Child")] يشتغل
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"🔴 JWT FAIL: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"🟢 JWT SUCCESS: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

// ================= Identity =================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // الطول الأدنى 6
    options.Password.RequiredLength = 6;

    // إلغاء كل شروط التعقيد تماماً
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ================= Caching =================
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// ================= Services =================
builder.Services.AddScoped<IGamingService, GamingService>();
builder.Services.AddScoped<IScoreObserver, BadgeLogic>();
builder.Services.AddScoped<IScoreObserver, ParentNotificationLogic>();
builder.Services.AddHostedService<DailyLogClosureService>();
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// ================= CORS =================
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader()));

// ================= Swagger =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Grawiaa API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ================= PIPELINE ترتيب مهم جدًا =================

// 1. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Static files
app.UseStaticFiles();

// 3. Routing (مهم جدًا)
app.UseRouting();

// 4. CORS
app.UseCors("AllowAll");

// 5. Authentication + Authorization (قبل controllers)
app.UseAuthentication();
app.UseAuthorization();

// 6. Controllers
app.MapControllers();


app.Run();