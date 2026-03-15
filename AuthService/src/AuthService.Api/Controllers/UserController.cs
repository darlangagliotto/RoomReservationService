using AuthService.Application.UseCases.FindUserById;
using AuthService.Application.UseCases.FindUserByEmail;
using AuthService.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IRegisterUserUseCase _registerUserUseCase;
        private readonly IFindUserByIdUseCase _findUserByIdUseCase;
        private readonly IFindUserByEmailUseCase _findUserByEmailUseCase;

        public UserController(
            IRegisterUserUseCase registerUserUseCase,
            IFindUserByIdUseCase findUserByIdUseCase,
            IFindUserByEmailUseCase findUserByEmailUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
            _findUserByIdUseCase = findUserByIdUseCase;
            _findUserByEmailUseCase = findUserByEmailUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
        {
            var response = await _registerUserUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Usuario ja cadastrado",
                    detail: response.Error,
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Value!.Id },
                response.Value
            );
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(FindUserByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FindUserByIdResponse>> GetById(Guid id)
        {
            var request = new FindUserByIdRequest(id);
            var response = await _findUserByIdUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Usuário não encontrado",
                    detail: response.Error,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Ok(response.Value);
        }

        [HttpGet("by-email")]
        [ProducesResponseType(typeof(FindUserByEmailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FindUserByEmailResponse>> GetByEmail([FromQuery] string email)
        {
            var request = new FindUserByEmailRequest(email);
            var response = await _findUserByEmailUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                if (response.Error == "É necessário informar o e-mail.")
                {
                    return Problem(
                        title: "E-mail inválido",
                        detail: response.Error,
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                return Problem(
                    title: "Usuário não encontrado",
                    detail: response.Error,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Ok(response.Value);
        }
    }
}