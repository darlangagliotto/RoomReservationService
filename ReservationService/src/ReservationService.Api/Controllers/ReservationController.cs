using ReservationService.Application.UseCases.CreateReservation;
using ReservationService.Application.UseCases.GetReservations;
using ReservationService.Application.UseCases.CancelReservation;
using ReservationService.Application.UseCases.GetAvailability;
using ReservationService.Application.UseCases.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReservationService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly ICreateReservationUseCase _createReservationUseCase;
        private readonly IGetReservationsUseCase _getReservationsUseCase;
        private readonly ICancelReservationUseCase _cancelReservationUseCase;
        private readonly IGetAvailabilityUseCase _getAvailabilityUseCase;

        public ReservationController(
            ICreateReservationUseCase createReservationUseCase,
            IGetReservationsUseCase getReservationsUseCase,
            ICancelReservationUseCase cancelReservationUseCase,
            IGetAvailabilityUseCase getAvailabilityUseCase)
        {
            _createReservationUseCase = createReservationUseCase;
            _getReservationsUseCase = getReservationsUseCase;
            _cancelReservationUseCase = cancelReservationUseCase;
            _getAvailabilityUseCase = getAvailabilityUseCase;
        }

        /// <summary>
        /// Estado de cada sala do andar no intervalo consultado.
        ///
        /// Devolve SEMPRE todas as salas, inclusive as sem reserva. Vazio e 200
        /// com [], e nao o 400 do ADR-011: "nao ha salas cadastradas" e resposta
        /// legitima. Ver docs/specs/003.
        /// </summary>
        [HttpGet("availability")]
        [ProducesResponseType(typeof(List<RoomAvailabilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<RoomAvailabilityResponse>>> GetAvailability(
            [FromQuery] GetAvailabilityRequest request)
        {
            var response = await _getAvailabilityUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Business error",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Ok(response.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateReservationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateReservationResponse>> Create([FromBody] CreateReservationRequest request)
        {
            var response = await _createReservationUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Business error",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return CreatedAtAction(
                nameof(Create),
                new { id = response.Value?.Reservation.Id },
                response.Value
            );
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ReservationResponse>>> GetReservations([FromQuery] GetReservationsRequest request)
        {
            var response = await _getReservationsUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Business error",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Ok(response.Value);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CancelReservationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CancelReservationResponse>> Cancel([FromRoute] Guid id)
        {
            var response = await _cancelReservationUseCase.ExecuteAsync(new CancelReservationRequest(id));

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Business error",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Ok(response.Value);
        }
    }
}
