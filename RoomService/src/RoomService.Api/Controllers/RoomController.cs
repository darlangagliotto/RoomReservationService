using RoomService.Application.UseCases.RegisterRoom;
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

        public RoomController(
            IRegisterRoomUseCase registerRoomUseCase)
        {
            _registerRoomUseCase = registerRoomUseCase;            
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
                new { id = response.Value?.Id},
                response.Value
            );
        }        
    }
}