using Microsoft.Extensions.DependencyInjection;
using RoomService.Application.UseCases.RegisterUser;
using RoomService.Application.UseCases.ValidateCredentials;
using FluentValidation;
using System.Reflection;

namespace RoomService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
            services.AddScoped<IValidateCredentialsUseCase, ValidateCredentialsUseCase>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}