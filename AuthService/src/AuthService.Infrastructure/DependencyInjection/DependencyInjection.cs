using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AuthService.Infrastructure.Security;
using AuthService.Infrastructure.Services;
using AuthService.Domain.Security;
using AuthService.Domain.Services;

namespace AuthService.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt settings not found!");

            jwtOptions.Validate();

            services.AddSingleton(jwtOptions);
            services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            
            // HttpClient para chamar UserService
            var userServiceUrl = configuration["Services:UserServiceUrl"] 
                ?? throw new InvalidOperationException("UserService URL not configured");
            
            services.AddHttpClient<IUserValidationService, UserValidationService>(client =>
            {
                client.BaseAddress = new Uri(userServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            
            return services;
        }
    }
}