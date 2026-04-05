using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRooms;
using RoomService.Application.UseCases.UpdateRoomDetails;
using RoomService.Application.UseCases.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RoomService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly IRegisterRoomUseCase _registerRoomUseCase;
        private readonly IGetRoomsUseCase _getRoomsUseCase;
        private readonly IUpdateRoomDetailsUseCase _updateRoomDetailsUseCase;

        public RoomController(
            IRegisterRoomUseCase registerRoomUseCase,
            IGetRoomsUseCase getRoomsUseCase,
            IUpdateRoomDetailsUseCase updateRoomDetailsUseCase)
        {
            _registerRoomUseCase = registerRoomUseCase;
            _getRoomsUseCase = getRoomsUseCase;
            _updateRoomDetailsUseCase = updateRoomDetailsUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterRoomResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterRoomResponse>> Register([FromBody] RegisterRoomRequest request)
        {
            var response = await _registerRoomUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Erro de negócio",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return CreatedAtAction(
                nameof(Register),
                new { id = response.Value?.Room.Id},
                response.Value
            );
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RoomResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<RoomResponse>>> GetRooms([FromQuery] GetRoomsRequest request)
        {
            var response = await _getRoomsUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Erro de negócio",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Ok(response.Value);
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(UpdateRoomDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UpdateRoomDetailsResponse>> UpdateDetails(
            [FromRoute] Guid id,
            [FromBody] UpdateRoomDetailsRequest request)
        {
            var response = await _updateRoomDetailsUseCase.ExecuteAsync(request with { RoomId = id });

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Erro de negócio",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            return Ok(response.Value);
        }    
    }
}