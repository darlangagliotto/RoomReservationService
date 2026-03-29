using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.RegisterEquipment
{
    public interface IRegisterEquipmentUseCase
    {
        Task<Result<RegisterEquipmentResponse>>  ExecuteAsync(RegisterEquipmentRequest request);
    }
}