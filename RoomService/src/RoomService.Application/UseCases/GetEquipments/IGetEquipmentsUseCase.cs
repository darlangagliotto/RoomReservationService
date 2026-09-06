using RoomService.Application.UseCases.Common;
using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetEquipments
{
    public interface IGetEquipmentsUseCase
    {
        Task<Result<List<EquipmentResponse>>> ExecuteAsync(GetEquipmentsRequest request);
    }
}
