using Microsoft.Extensions.DependencyInjection;
using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRoomByNumber;
using RoomService.Application.UseCases.GetRoomByName;
using RoomService.Application.UseCases.RegisterEquipment;
using RoomService.Application.UseCases.GetAllRooms;
using RoomService.Application.UseCases.UpdateRoomDetails;
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
            services.AddScoped<IRegisterEquipmentUseCase, RegisterEquipmentUseCase>();
            services.AddScoped<IGetRoomByNumberUseCase, GetRoomByNumberUseCase>();
            services.AddScoped<IGetRoomByNameUseCase, GetRoomByNameUseCase>();
            services.AddScoped<IGetAllRoomsUseCase, GetAllRoomsUseCase>();
            services.AddScoped<IUpdateRoomDetailsUseCase, UpdateRoomDetailsUseCase>();
            services.AddScoped<IEquipmentResponseMapper, EquipmentResponseMapper>();
            
            return services;
        }
    }
}