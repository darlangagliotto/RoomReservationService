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

        public UserController(
            IRegisterUserUseCase registerUserUseCase,
            IValidateCredentialsUseCase validateCredentialsUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
            _validateCredentialsUseCase = validateCredentialsUseCase;
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