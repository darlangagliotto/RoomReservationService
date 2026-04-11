using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.RegisterEquipment;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using System.IO.Pipelines;

namespace RoomService.Application.UseCases.RegisterRoom
{
    public class RegisterRoomUseCase : IRegisterRoomUseCase
    {
        private readonly IRoomRepository _roomRepository;

        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public RegisterRoomUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper,
            IEquipmentRepository equipmentRepository)
        {
            _roomRepository = roomRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<RegisterRoomResponse>> ExecuteAsync(RegisterRoomRequest request)
        {
            var validationResult = await ValidateAsync(request);
            if (!validationResult.IsSuccess)
            {
                return Result<RegisterRoomResponse>.Failure(validationResult.Error!);
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

        private async Task<Result<bool>> ValidateAsync(RegisterRoomRequest request)
        {
            var roomUniquenessValidation = await ValidateRoomUniquenessAsync(request);
            if (!roomUniquenessValidation.IsSuccess)
            {
                return roomUniquenessValidation;
            }

            var equipmentIdsValidation = await ValidateEquipmentIdsAsync(request);
            if (!equipmentIdsValidation.IsSuccess)
            {
                return equipmentIdsValidation;
            }

            var equipmnetAllocationValidation = await ValidateEquipmentAllocationAsync(request.EquipmentIds);
            if (!equipmnetAllocationValidation.IsSuccess)
            {
                return equipmentIdsValidation;
            }

            return Result<bool>.Success(true);
        }

       private async Task<Result<bool>> ValidateRoomUniquenessAsync(RegisterRoomRequest request)
        {
            var existingRoom = await _roomRepository.GetByNameOrNumberAsync(request.Name, request.Number);

            if (existingRoom is not null)
            {
                return Result<bool>.Failure("Room is already registered.");
            }
            return Result<bool>.Success(true);
        }

        private async Task<Result<bool>> ValidateEquipmentIdsAsync(RegisterRoomRequest request)
        {    
            foreach (var equipmentId in request.EquipmentIds.Distinct())
            {
                var existEquipment = await _equipmentRepository.GetByIdAsync(equipmentId);

                if (existEquipment is null)
                {
                    return Result<bool>.Failure($"Equipment with ID {equipmentId} not found!");    
                }
            }

            if (request.EquipmentIds.Any(id => id == Guid.Empty))
            {
                return Result<bool>.Failure("There is equipment with an invalid ID.");
            }

            return Result<bool>.Success(true);
        }

        private async Task<Result<bool>> ValidateEquipmentAllocationAsync(IEnumerable<Guid> equipmentIds)
        {
            foreach (var equipmentId in equipmentIds.Distinct())
            {
                var alreadyAllocated = await _roomRepository.ExistsByEquipmentIdAsync(equipmentId);
                if (alreadyAllocated)
                    return Result<bool>.Failure($"Equipment {equipmentId} is already allocated to another room.");
            }
            return Result<bool>.Success(true);
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