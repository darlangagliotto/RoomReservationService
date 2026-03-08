using AuthService.Application.UseCases.LoginUser;
using AuthService.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IRegisterUserUseCase _registerUserUseCase;
        private readonly ILoginUserUseCase _loginUserUseCase;

        public UserController(IRegisterUserUseCase registerUserUseCase, ILoginUserUseCase loginUserUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
            _loginUserUseCase = loginUserUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
        {
            var response = await _registerUserUseCase.ExecuteAsync(request);

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

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginUserResponse>> Login([FromBody] LoginUserRequest request)
        {
            var response = await _loginUserUseCase.ExecuteAsync(request);

            if(!response.IsSuccess)
            {
                return Problem(
                    title: "Erro de negócio",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return CreatedAtAction(
                nameof(Login),
                new { email = request.Email },
                response.Value
            );
        }
    }
}