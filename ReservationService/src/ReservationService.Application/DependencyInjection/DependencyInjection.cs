using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using ReservationService.Application.UseCases.GetReservations;
using ReservationService.Application.Services;

namespace ReservationService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IGetReservationsUseCase, GetReservationsUseCase>();
            services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>
            {
                client.BaseAddress = new Uri("http://roomservice:5000");
            });
            services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri("http://userservice:5000");
            });
            return services;
        }
    }
}