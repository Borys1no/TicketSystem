using Microsoft.AspNetCore.Mvc;
using SupportFlow.Application.Commands.Tickets;
using SupportFlow.Application.DTOs.Tickets;
using SupportFlow.Application.Mappers;
using System;
using SupportFlow.Application.Queries.Tickets;

namespace SupportFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase

{
    private readonly CreateTicketCommandHandler _createHandler;
    private readonly GetTicketsQueryHandler _getTicketsHandler;

    public TicketsController(CreateTicketCommandHandler createHandler,
        GetTicketsQueryHandler getTicketsHandler)
    {
        _createHandler = createHandler;
        _getTicketsHandler = getTicketsHandler;
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
        try
        {
            var ticket = await _createHandler.Handle(command);
            var response = TicketMapper.ToResponse(ticket);
            return Created($"/api/tickets/{ticket.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }

        
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _getTicketsHandler.Handle();
        var response = tickets.Select(TicketMapper.ToResponse);
        return Ok (response);
    }

}