using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetRoomByName
{
    public interface IGetRoomByNameUseCase
    {
        Task<Result<GetRoomByNameResponse>> ExecuteAsync(GetRoomByNameRequest request);
    }
}