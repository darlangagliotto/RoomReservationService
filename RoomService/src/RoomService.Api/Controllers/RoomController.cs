using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRoomByNumber;
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
        private readonly IGetRoomByNumberUseCase _getRoomByNumberUseCase;

        public RoomController(
            IRegisterRoomUseCase registerRoomUseCase,
            IGetRoomByNumberUseCase getRoomByNumberUseCase)
        {
            _registerRoomUseCase = registerRoomUseCase;
            _getRoomByNumberUseCase = getRoomByNumberUseCase;
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

        [HttpGet("by-number")]
        [ProducesResponseType(typeof(GetRoomByNumberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetRoomByNumberResponse>> GetByRoomNumber([FromQuery] GetRoomByNumberRequest request)
        {
            var response = await _getRoomByNumberUseCase.ExecuteAsync(request);

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