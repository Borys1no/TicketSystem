using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Users;
using SupportFlow.Application.DTOs.Users;

namespace SupportFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UserController : ControllerBase
{
    private readonly CreateUserCommandHandler _handler;

    public UserController(CreateUserCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var command = new CreateUserCommand()
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = request.Password,
            Department = request.Department,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role
        };

        await _handler.Handle(command);
        return Ok();
    }
}

