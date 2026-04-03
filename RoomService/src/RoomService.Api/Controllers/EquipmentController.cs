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

        public EquipmentController(
            IRegisterEquipmentUseCase registerEquipmentUseCase)
        {
            _registerEquipmentUseCase = registerEquipmentUseCase;
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
                new { id = response.Value?.Id },
                response.Value
            );
        }
    }
}
