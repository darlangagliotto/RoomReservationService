using AuthService.Application.UseCases.LoginUser;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginUserUseCase _loginUserUseCase;

        public AuthController(ILoginUserUseCase loginUserUseCase)
        {            
            _loginUserUseCase = loginUserUseCase;
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
                    title: "Business error",
                    detail: response.Error,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Ok(response.Value);
        }
    }
}