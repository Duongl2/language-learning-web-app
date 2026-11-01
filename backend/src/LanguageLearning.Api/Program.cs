
using System.Text;
using LanguageLearning.Api.Authorization;
using LanguageLearning.Api.Middleware;
using LanguageLearning.Api.Services;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey must contain at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ??
                "LanguageLearningApp",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ??
                "LanguageLearningAppClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireRole(
            UserRole.Learner.ToString(),
            UserRole.Admin.ToString())
        .Build();
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(UserRole.Admin.ToString()));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<AdminContentService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
