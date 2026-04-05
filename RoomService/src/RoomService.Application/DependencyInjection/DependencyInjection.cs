using Microsoft.Extensions.DependencyInjection;
using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.RegisterEquipment;
using RoomService.Application.UseCases.GetRooms;
using RoomService.Application.UseCases.UpdateRoomDetails;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRegisterRoomUseCase, RegisterRoomUseCase>();
            services.AddScoped<IRegisterEquipmentUseCase, RegisterEquipmentUseCase>();
            services.AddScoped<IGetRoomsUseCase, GetRoomsUseCase>();
            services.AddScoped<IUpdateRoomDetailsUseCase, UpdateRoomDetailsUseCase>();
            services.AddScoped<IEquipmentResponseMapper, EquipmentResponseMapper>();
            
            return services;
        }
    }
}