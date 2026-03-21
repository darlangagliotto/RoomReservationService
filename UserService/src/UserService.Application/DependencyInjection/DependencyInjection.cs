using Microsoft.Extensions.DependencyInjection;
using UserService.Application.UseCases.RegisterUser;
using FluentValidation;
using System.Reflection;

namespace UserService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}