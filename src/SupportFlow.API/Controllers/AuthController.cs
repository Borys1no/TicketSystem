using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Auth;



namespace SupportFlow.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;

    public AuthController(LoginCommandHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        try
        {
            var token = await _loginHandler.Handle(command);

            return Ok(new
            {
               token
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }
}