
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using LanguageLearning.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageLearning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=language_learning_db;User=app_user;Password=app_password_123;TreatTinyAsBoolean=true;";

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IDeckRepository, DeckRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IAdminContentRepository, AdminContentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHashService, Pbkdf2PasswordHashService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        return services;
    }
}
