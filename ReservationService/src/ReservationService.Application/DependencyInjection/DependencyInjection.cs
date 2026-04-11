using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace ReservationService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
