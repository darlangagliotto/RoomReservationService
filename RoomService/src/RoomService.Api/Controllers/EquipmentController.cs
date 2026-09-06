using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.GetEquipments;
using RoomService.Application.UseCases.RegisterEquipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RoomService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/equipments")]
    public class EquipmentController : ControllerBase
    {
        private readonly IRegisterEquipmentUseCase _registerEquipmentUseCase;
        private readonly IGetEquipmentsUseCase _getEquipmentsUseCase;

        public EquipmentController(
            IRegisterEquipmentUseCase registerEquipmentUseCase,
            IGetEquipmentsUseCase getEquipmentsUseCase)
        {
            _registerEquipmentUseCase = registerEquipmentUseCase;
            _getEquipmentsUseCase = getEquipmentsUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterEquipmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterEquipmentResponse>> Register([FromBody] RegisterEquipmentRequest request)
        {
            var response = await _registerEquipmentUseCase.ExecuteAsync(request);

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
                new { id = response.Value?.Equipment.Id },
                response.Value
            );
        }

        /// <summary>
        /// Listagem, com filtros opcionais por tipo e por nao alocado.
        ///
        /// Vazio devolve 200 com lista vazia, e nao o 400 de "No X found" do
        /// ADR-011: ausencia de equipamento e resposta legitima. Ver docs/specs/004.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EquipmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<EquipmentResponse>>> GetEquipments([FromQuery] GetEquipmentsRequest request)
        {
            var response = await _getEquipmentsUseCase.ExecuteAsync(request);

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
