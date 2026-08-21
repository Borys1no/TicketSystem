using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Users;
using SupportFlow.Application.DTOs.Users;
using SupportFlow.Application.Queries.Users;

namespace SupportFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UsersController : ControllerBase
{
    private readonly CreateUserCommandHandler _handler;
    private readonly CreateUserCommandHandler _createUserHandler;
    private readonly GetUserByIdQueryHandler _getUserByIdQueryHandler;

    public UsersController(CreateUserCommandHandler handler,
        CreateUserCommandHandler createUserHandler,
        GetUserByIdQueryHandler getUserByIdQueryHandler
        )
    {
        _handler = handler;
        _createUserHandler = createUserHandler;
       _getUserByIdQueryHandler = getUserByIdQueryHandler;
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

        var user = await _handler.Handle(command);
        var response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department.ToString(),
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            };
        return Created($"api/users/{user.Id}", response);

    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var user = await _getUserByIdQueryHandler.Handle(query);
        if (user is null)
            return NotFound();
        var response = new UserResponse()
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            Department = user.Department.ToString(),
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString()
        };
        return Ok(response);
    }
}

