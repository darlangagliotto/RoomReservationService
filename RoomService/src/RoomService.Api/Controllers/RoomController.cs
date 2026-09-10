using RoomService.Application.UseCases.RegisterRoom;
using RoomService.Application.UseCases.GetRoomById;
using RoomService.Application.UseCases.GetRooms;
using RoomService.Application.UseCases.UpdateRoomDetails;
using RoomService.Application.UseCases.AssignEquipmentsToRoom;
using RoomService.Application.UseCases.RemoveEquipmentFromRoom;
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
        private readonly IGetRoomByIdUseCase _getRoomByIdUseCase;
        private readonly IAssignEquipmentsToRoomUseCase _assignEquipmentsToRoomUseCase;
        private readonly IRemoveEquipmentFromRoomUseCase _removeEquipmentFromRoomUseCase;

        public RoomController(
            IRegisterRoomUseCase registerRoomUseCase,
            IGetRoomsUseCase getRoomsUseCase,
            IUpdateRoomDetailsUseCase updateRoomDetailsUseCase,
            IGetRoomByIdUseCase getRoomByIdUseCase,
            IAssignEquipmentsToRoomUseCase assignEquipmentsToRoomUseCase,
            IRemoveEquipmentFromRoomUseCase removeEquipmentFromRoomUseCase)
        {
            _registerRoomUseCase = registerRoomUseCase;
            _getRoomsUseCase = getRoomsUseCase;
            _updateRoomDetailsUseCase = updateRoomDetailsUseCase;
            _getRoomByIdUseCase = getRoomByIdUseCase;
            _assignEquipmentsToRoomUseCase = assignEquipmentsToRoomUseCase;
            _removeEquipmentFromRoomUseCase = removeEquipmentFromRoomUseCase;
        }

        /// <summary>
        /// Consulta por identificador.
        ///
        /// Ausencia responde 404, e nao o 400 de negocio do ADR-011: o chamador
        /// precisa distinguir "nao existe" de "falhou". Ver docs/specs/002.
        ///
        /// Busca por nome ou numero continua em GET /api/rooms?name=&amp;number=;
        /// nao existem rotas /name/{n} nem /number/{n}.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomResponse>> GetById([FromRoute] Guid id)
        {
            var response = await _getRoomByIdUseCase.ExecuteAsync(new GetRoomByIdRequest(id));

            if (!response.IsSuccess)
            {
                return NotFound();
            }

            return Ok(response.Value);
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

        [HttpPost("{id:guid}/equipments")]
        [ProducesResponseType(typeof(AssignEquipmentsToRoomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssignEquipmentsToRoomResponse>> AssignEquipments(
            [FromRoute] Guid id,
            [FromBody] AssignEquipmentsToRoomRequest request)
        {
            var response = await _assignEquipmentsToRoomUseCase.ExecuteAsync(request with { RoomId = id });

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

        [HttpDelete("{id:guid}/equipments/{equipmentId:guid}")]
        [ProducesResponseType(typeof(RemoveEquipmentFromRoomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RemoveEquipmentFromRoomResponse>> RemoveEquipment(
            [FromRoute] Guid id,
            [FromRoute] Guid equipmentId)
        {
            var response = await _removeEquipmentFromRoomUseCase.ExecuteAsync(
                new RemoveEquipmentFromRoomRequest(id, equipmentId));

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