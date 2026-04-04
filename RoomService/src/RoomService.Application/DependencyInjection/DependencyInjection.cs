using Microsoft.Extensions.DependencyInjection;
using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRoomByNumber;
using RoomService.Application.UseCases.RegisterEquipment;
using RoomService.Application.UseCases.Common.Services;
using FluentValidation;
using System.Reflection;

namespace RoomService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterRoomUseCase, RegisterRoomUseCase>();
            services.AddScoped<IGetRoomByNumberUseCase, GetRoomByNumberUseCase>();
            services.AddScoped<IEquipmentResponseMapper, EquipmentResponseMapper>();
            services.AddScoped<IRegisterEquipmentUseCase, RegisterEquipmentUseCase>();            
            return services;
        }
    }
}