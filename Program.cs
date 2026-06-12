using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillifyAPI.ZegoService;
using SkillifyAPI.CloudinaryService;
using SkillifyAPI.Data;
using SkillifyAPI.JwtService;
using SkillifyAPI.Repositories.BadgeRepository;
using SkillifyAPI.Repositories.LanguageRepository;
using SkillifyAPI.Repositories.MainSkillRepository;
using SkillifyAPI.Repositories.SubSkillRepository;
using SkillifyAPI.Repositories.UserRepository;
using SkillifyAPI.Repositories.SessionRepository;
using SkillifyAPI.Services.BadgeService;
using SkillifyAPI.Services.LanguageService;
using SkillifyAPI.Services.MainSkillService;
using SkillifyAPI.Services.SessionService;
using SkillifyAPI.Services.SubSkillService;
using SkillifyAPI.Services.UserService;
using SkillifyAPI.Validations.UserValidation;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));
// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// ── Swagger / OpenAPI (Swashbuckle) ───────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1.1", new OpenApiInfo
    {
        Title = "Skillify API",
        Version = "v1.1",
        Description = "Peer-to-peer skill exchange platform API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    options.EnableAnnotations();
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── JWT authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var repo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var sidClaim = context.Principal?.FindFirst("sid")?.Value;

            if (string.IsNullOrEmpty(sidClaim))
            {
                context.Fail("Unauthorized: Invalid session identifier.");
                return;
            }

            var refreshToken = await repo.GetRefreshTokenAsync(sidClaim);
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                context.Fail("Unauthorized: Token has been revoked or expired.");
            }
        }
    };
});

builder.Services.AddAuthorization();

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMainSkillRepository, MainSkillRepository>();
builder.Services.AddScoped<IMainSkillService, MainSkillService>();
builder.Services.AddScoped<ISubSkillRepository, SubSkillRepository>();
builder.Services.AddScoped<ISubSkillService, SubSkillService>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ── CORS (adjust origins for production) ──────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    // Global 429 response
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
            ? (int)retry.TotalSeconds : 60;

        context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = 429,
            error = "Too Many Requests",
            message = $"Rate limit exceeded. Please try again in {retryAfter} seconds."
        }, cancellationToken);
    };

    // ── Fixed Window Policy ──────────────────────────────────
    //    50 requests per 1 minute per client key
    options.AddPolicy("FixedWindow", httpContext =>
    {
        // If authenticated → use User ID as key
        // If anonymous    → fall back to IP address
        var clientKey = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? httpContext.Connection.RemoteIpAddress?.ToString()
              ?? "unknown"
            : httpContext.Connection.RemoteIpAddress?.ToString()
              ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientKey, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 50,          // max requests
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0             // no queuing — reject immediately
            });
    });
});



builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Register your services
builder.Services.AddScoped<IZegoTokenService, ZegoTokenService>();
builder.Services.AddScoped<IZegoRoomService, ZegoRoomService>();
builder.Services.AddScoped<ISessionMeetingService, SessionMeetingService>();





var app = builder.Build();

// ── HTTP pipeline ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Skillify API v1.1");
        options.DocumentTitle = "Skillify API";
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await context.Database.MigrateAsync();

    await DataSeeder.Seed(context);
}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate(); // runs pending migrations automatically
//}

// In app pipeline
app.UseHangfireDashboard("/hangfire"); // optional, remove in prod or auth-gate it


app.UseStaticFiles(); // serves wwwroot/WEB_UIKITS.html

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();   // ← must be AFTER app.UseAuthentication() and app.UseAuthorization()
app.MapControllers().RequireRateLimiting("FixedWindow");

app.Run();

