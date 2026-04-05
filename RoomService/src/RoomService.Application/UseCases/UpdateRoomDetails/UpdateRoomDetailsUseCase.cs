using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Domain.Common;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.UpdateRoomDetails
{
    public class UpdateRoomDetailsUseCase : IUpdateRoomDetailsUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public UpdateRoomDetailsUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }
        
        public async Task<Result<UpdateRoomDetailsResponse>> ExecuteAsync(UpdateRoomDetailsRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId);

            if (room is null)
            {
                return Result<UpdateRoomDetailsResponse>.Failure("Sala não encontrada!");
            }

            var validationIsOk = await ValidateAsync(request);

            if (!validationIsOk.IsSuccess)
            {
                return Result<UpdateRoomDetailsResponse>.Failure(validationIsOk.Error!);
            }            
            
            if (request.Name is not null)
            {
                room.Rename(request.Name);
            }

            if (request.Number.HasValue)
            {
                room.ChangeNumber(request.Number.Value);
            }

            await _roomRepository.UpdateAsync(room);

            var equipments = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<UpdateRoomDetailsResponse>.Success(
                new UpdateRoomDetailsResponse(
                    new RoomResponse(
                        room.Id,
                        room.Name,
                        room.Number,
                        equipments
                    )
                )
            );
        }

        private async Task<Result<bool>> ValidateAsync(UpdateRoomDetailsRequest request)
        {
           if (request.Name is null && !request.Number.HasValue)
           {
                return Result<bool>.Failure("Informe ao menos um campo para atualização!");
           }

           if (request.Name is not null)
           {
                var sameNameRoom = await _roomRepository.GetByNameAsync(request.Name);
                if (sameNameRoom is not null && sameNameRoom.Id != request.RoomId)
                {
                    return Result<bool>.Failure("Já existe uma sala com esse nome.");  
                }
           }

           if (request.Number.HasValue)
            {
                var sameNumberRoom = await _roomRepository.GetByNumberAsync(request.Number.Value);
                if (sameNumberRoom is not null && sameNumberRoom.Id != request.RoomId)
                {
                    return Result<bool>.Failure("Já existe uma sala com esse número.");
                }                
            }
            return Result<bool>.Success(true);
        }
    }
}