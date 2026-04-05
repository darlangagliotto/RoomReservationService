using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.UpdateRoomDetails
{
    public interface IUpdateRoomDetailsUseCase
    {
         Task<Result<UpdateRoomDetailsResponse>> ExecuteAsync(UpdateRoomDetailsRequest request);
    }
}