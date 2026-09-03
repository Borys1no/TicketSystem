using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Users;
using SupportFlow.Application.DTOs.Users;
using SupportFlow.Application.Queries.Users;
using SupportFlow.Application.Mappers;

namespace SupportFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Adminitrador")]

public class UsersController : ControllerBase
{
    private readonly CreateUserCommandHandler _handler;
    private readonly GetUserByIdQueryHandler _getUserByIdQueryHandler;
    private readonly GetUsersQueryHandler _getUsersHandler;

    public UsersController(CreateUserCommandHandler handler,
        GetUserByIdQueryHandler getUserByIdQueryHandler,
        GetUsersQueryHandler getUsersHandler
        )
    {
        _handler = handler;
       _getUserByIdQueryHandler = getUserByIdQueryHandler;
       _getUsersHandler = getUsersHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var command = new CreateUserCommand()
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            Department = request.Department,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role
        };

        var user = await _handler.Handle(command);
        var response = UserMapper.ToResponse(user);
        return Created($"api/users/{user.Id}", response);

    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var user = await _getUserByIdQueryHandler.Handle(query);
        if (user is null)
            return NotFound();
        var response = UserMapper.ToResponse(user);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetUsersQuery();
        var users = await _getUsersHandler.Handle(query);
        var response = users.Select(UserMapper.ToResponse);
        return Ok(response);
    }
}

