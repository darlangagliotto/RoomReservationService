using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRoomByNumber;
using RoomService.Application.UseCases.GetRoomByName;
using RoomService.Application.UseCases.GetAllRooms;
using RoomService.Application.UseCases.UpdateRoomDetails;
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
        private readonly IGetRoomByNameUseCase _getRoomByNameUseCase;
        private readonly IGetAllRoomsUseCase _getAllRoomsUseCase;
        private readonly IUpdateRoomDetailsUseCase _updateRoomDetailsUseCase;

        public RoomController(
            IRegisterRoomUseCase registerRoomUseCase,
            IGetRoomByNumberUseCase getRoomByNumberUseCase,
            IGetAllRoomsUseCase getAllRoomsUseCase,
            IGetRoomByNameUseCase getRoomByNameUseCase,
            IUpdateRoomDetailsUseCase updateRoomDetailsUseCase)
        {
            _registerRoomUseCase = registerRoomUseCase;
            _getRoomByNumberUseCase = getRoomByNumberUseCase;
            _getRoomByNameUseCase = getRoomByNameUseCase;
            _getAllRoomsUseCase = getAllRoomsUseCase;
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

        [HttpGet("by-number")]
        [ProducesResponseType(typeof(GetRoomByNumberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetRoomByNumberResponse>> GeRoomByNumber([FromQuery] GetRoomByNumberRequest request)
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

        [HttpGet("by-name")]
        [ProducesResponseType(typeof(GetRoomByNameResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetRoomByNameResponse>> GetRoomByName([FromQuery] GetRoomByNameRequest request)
        {
            var response = await _getRoomByNameUseCase.ExecuteAsync(request);

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

        [HttpGet("all")]
        [ProducesResponseType(typeof(GetAllRoomsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<GetAllRoomsResponse>>> GetAllRooms()
        {
            var response = await _getAllRoomsUseCase.ExecuteAsync();

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

        [HttpPatch("{id:guid}/details")]
        [ProducesResponseType(typeof(UpdateRoomDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UpdateRoomDetailsResponse>> UpdateDetails(
            [FromRoute] Guid id,
            [FromBody] UpdateRoomDetailsRequest request)
        {
            var response = await _updateRoomDetailsUseCase.ExecuteAsync(request);

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