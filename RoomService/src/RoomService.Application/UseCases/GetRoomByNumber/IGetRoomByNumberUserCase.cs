using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetRoomByNumber
{
    public interface IGetRoomByNumberUseCase
    {
        Task<Result<GetRoomByNumberResponse>> ExecuteAsync(GetRoomByNumberRequest request);
    }
}