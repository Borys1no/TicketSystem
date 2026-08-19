using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Tickets;
using SupportFlow.Application.DTOs.Tickets;

namespace SupportFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase

{
    private readonly CreateTicketCommandHandler _handler;

    public TicketsController(CreateTicketCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTicketRequest request)
    {
        var command = new CreateTicketCommand
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CreatedByUserId = request.CreatedByUserId
        };

        await _handler.Handle(command);

        return Ok();
    }

}