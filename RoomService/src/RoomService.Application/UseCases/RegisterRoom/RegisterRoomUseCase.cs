using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.RegisterEquipment;

namespace RoomService.Application.UseCases.RegisterRoom
{
    public class RegisterRoomUseCase : IRegisterRoomUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public RegisterRoomUseCase(
            IRoomRepository roomRepository,
            IEquipmentRepository equipmentRepository)
        {
            _roomRepository = roomRepository;
            _equipmentRepository = equipmentRepository;
        }

        public async Task<Result<RegisterRoomResponse>> ExecuteAsync(RegisterRoomRequest request)
        {
            var existingRoom = await _roomRepository.GetByNameAndNumberAsync(request.Name, request.Number);

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

            var equipmentResponses = await MapEquipmentsAsync(room.Equipments);

            return Result<RegisterRoomResponse>.Success(
                new RegisterRoomResponse(
                    room.Id,
                    room.Name,
                    room.Number,
                    equipmentResponses
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

        private async Task<List<RegisterEquipmentResponse>> MapEquipmentsAsync(IEnumerable<RoomEquipment> roomEquipments)
        {
            var equipmentResponses = new List<RegisterEquipmentResponse>();
            foreach (var roomEquipment in roomEquipments)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(roomEquipment.EquipmentId);
                equipmentResponses.Add(new RegisterEquipmentResponse(
                    equipment.Id,
                    equipment.Type,
                    equipment.Brand,
                    equipment.SerialNumber,
                    equipment.PurchaseDate
                ));
            }
            return equipmentResponses;
        }

    }
}