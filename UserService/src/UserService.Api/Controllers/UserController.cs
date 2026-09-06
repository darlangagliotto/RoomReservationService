using UserService.Application.UseCases.GetUserById;
using UserService.Application.UseCases.RegisterUser;
using UserService.Application.UseCases.ValidateCredentials;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IRegisterUserUseCase _registerUserUseCase;
        private readonly IValidateCredentialsUseCase _validateCredentialsUseCase;
        private readonly IGetUserByIdUseCase _getUserByIdUseCase;

        public UserController(
            IRegisterUserUseCase registerUserUseCase,
            IValidateCredentialsUseCase validateCredentialsUseCase,
            IGetUserByIdUseCase getUserByIdUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
            _validateCredentialsUseCase = validateCredentialsUseCase;
            _getUserByIdUseCase = getUserByIdUseCase;
        }

        /// <summary>
        /// Consulta por identificador.
        ///
        /// Ausencia responde 404, e nao o 400 de negocio do ADR-011: o chamador
        /// precisa distinguir "nao existe" de "falhou", e 404 e a resposta correta
        /// para recurso enderecado por id. Ver docs/specs/002.
        ///
        /// Exige autenticacao — e a primeira rota deste servico que exige.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetUserByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserByIdResponse>> GetById([FromRoute] Guid id)
        {
            var response = await _getUserByIdUseCase.ExecuteAsync(new GetUserByIdRequest(id));

            if (!response.IsSuccess)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
        {
            var response = await _registerUserUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Problem(
                    title: "Business error",
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

        [HttpPost("validate-credentials")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ValidateCredentialsResponse>> ValidateCredentials([FromBody] ValidateCredentialsRequest request)
        {
            var response = await _validateCredentialsUseCase.ExecuteAsync(request);

            if (!response.IsSuccess)
            {
                return Ok(new ValidateCredentialsResponse(false, null));
            }

            return Ok(response.Value);
        }        
    }
}