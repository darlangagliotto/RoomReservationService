using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.UseCases.RegisterUser;
using AuthService.Application.UseCases.LoginUser;
using FluentValidation;
using System.Reflection;

namespace AuthService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}