using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.Security;
using UserService.Domain.ValueObjects;

namespace UserService.Api.Seeding
{
    /// <summary>
    /// Creates a fixed user on startup so the system can be exercised right after
    /// <c>docker compose up</c>, without registering an account by hand.
    ///
    /// Two guards keep this out of production: it exits unless the environment is
    /// Development, and it can be switched off with Seed:DefaultUser:Enabled.
    /// A known credential in any other environment would be a backdoor account.
    ///
    /// Idempotent: an existing e-mail short-circuits, so restarts over a populated
    /// volume are harmless and the password is never silently reset.
    /// </summary>
    public static class DevelopmentSeeder
    {
        private const string SectionName = "Seed:DefaultUser";
        private const string DefaultName = "Administrador";
        private const string DefaultEmail = "admin@admin.com";
        private const string DefaultPassword = "admin";

        public static async Task SeedDefaultUserAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(DevelopmentSeeder));

            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            var section = app.Configuration.GetSection(SectionName);

            if (!section.GetValue("Enabled", true))
            {
                logger.LogInformation("Development seed is disabled by configuration.");
                return;
            }

            var name = section["Name"] ?? DefaultName;
            var email = section["Email"] ?? DefaultEmail;
            var password = section["Password"] ?? DefaultPassword;

            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            if (await repository.GetByEmailAsync(email) is not null)
            {
                logger.LogInformation("Development user {Email} already exists.", email);
                return;
            }

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User(name, new Email(email), passwordHasher.Hash(password));

            await repository.AddSync(user);

            logger.LogWarning(
                "Development user {Email} created with a well-known password. Never enable this outside Development.",
                email);
        }
    }
}
