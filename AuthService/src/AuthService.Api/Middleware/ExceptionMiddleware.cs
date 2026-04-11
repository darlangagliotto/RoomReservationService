using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var detail = _env.IsDevelopment()
                    ? $"{ex.GetType().Name}: {ex.Message}"
                    : "An unexpected error occurred.";

                var problem = new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = detail,
                    Status = 500,
                    Instance = context.Request.Path
                };
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}