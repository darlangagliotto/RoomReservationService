using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using ReservationService.Application.UseCases.GetReservations;
using ReservationService.Application.UseCases.CreateReservation;
using ReservationService.Application.UseCases.CancelReservation;
using ReservationService.Application.UseCases.GetAvailability;
using ReservationService.Application.Services;

namespace ReservationService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IGetReservationsUseCase, GetReservationsUseCase>();
            services.AddScoped<ICreateReservationUseCase, CreateReservationUseCase>();
            services.AddScoped<ICancelReservationUseCase, CancelReservationUseCase>();
            services.AddScoped<IGetAvailabilityUseCase, GetAvailabilityUseCase>();

            // URLs vem da configuracao: fixas no codigo, o servico so funcionava
            // dentro da rede do Compose. Ver docs/specs/002 e backlog B10.
            var roomServiceUrl = configuration["Services:RoomServiceUrl"]
                ?? throw new InvalidOperationException("RoomService URL not configured");
            var userServiceUrl = configuration["Services:UserServiceUrl"]
                ?? throw new InvalidOperationException("UserService URL not configured");

            services.AddTransient<AuthorizationPropagationHandler>();

            services.AddHttpClient<IRoomServiceClient, RoomServiceClient>(client =>
            {
                client.BaseAddress = new Uri(roomServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            }).AddHttpMessageHandler<AuthorizationPropagationHandler>();

            services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(userServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            }).AddHttpMessageHandler<AuthorizationPropagationHandler>();

            return services;
        }
    }
}
