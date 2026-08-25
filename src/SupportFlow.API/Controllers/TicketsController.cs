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
    private readonly GetTicketByIdQueryHandler _getTicketByIdQueryHandler;

    public TicketsController(
        CreateTicketCommandHandler createHandler,
        GetTicketsQueryHandler getTicketsHandler,
        GetTicketByIdQueryHandler getTicketByIdQueryHandler)
    {
        _createHandler = createHandler;
        _getTicketsHandler = getTicketsHandler;
        _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _getTicketByIdQueryHandler.Handle(id);
        if (ticket is null)
            return NotFound(new
            {
                message = "Ticket not found."
            });
        var response = TicketMapper.ToResponse(ticket);
        return Ok(response);
    }

}