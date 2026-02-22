using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.UseCases.RegisterUser;
using FluentValidation;

namespace AuthService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            // AutoMapper removido
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            return services;
        }
    }
}