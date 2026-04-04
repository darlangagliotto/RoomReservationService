using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.RegisterEquipment;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.UseCases.RegisterRoom
{
    public class RegisterRoomUseCase : IRegisterRoomUseCase
    {
        private readonly IRoomRepository _roomRepository;        
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public RegisterRoomUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;            
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<RegisterRoomResponse>> ExecuteAsync(RegisterRoomRequest request)
        {
            var existingRoom = await _roomRepository.GetByNameOrNumberAsync(request.Name, request.Number);

            if (existingRoom is not null)
            {
                return Result<RegisterRoomResponse>
                    .Failure("Sala já cadastrado!");
            }

            Room room;
            try
            {
                room = CreateRoom(request);
            }
            catch (DomainException ex)
            {
                return Result<RegisterRoomResponse>.Failure(ex.Message);
            }

            await _roomRepository.AddSync(room);

            var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<RegisterRoomResponse>.Success(
                new RegisterRoomResponse(
                    new RoomResponse(
                    room.Id,
                    room.Name,
                    room.Number,
                    equipmentResponses
                    )
                )
            );            
        }
        
        private Room CreateRoom(RegisterRoomRequest request)
        {
            var room = new Room(request.Name, request.Number);
            foreach (var equipmentId in request.EquipmentIds)
            {
                room.AddEquipment(equipmentId);
            }
            return room;
        }
    }
}