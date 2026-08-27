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
    private readonly ResolveTicketCommandHandler _resolveHandler;
    private readonly AssignTicketCommandHandler _assignHandler;
    private readonly CloseTicketCommandHandler _closeHandler;
    private readonly ReopenTicketCommandHandler _reopenHandler;

    public TicketsController(
        CreateTicketCommandHandler createHandler,
        GetTicketsQueryHandler getTicketsHandler,
        GetTicketByIdQueryHandler getTicketByIdQueryHandler,
        ResolveTicketCommandHandler resolveHandler,
        AssignTicketCommandHandler assignHandler,
        CloseTicketCommandHandler closeHandler,
        ReopenTicketCommandHandler reopenHandler)
    {
        _createHandler = createHandler;
        _getTicketsHandler = getTicketsHandler;
        _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
        _resolveHandler = resolveHandler;
        _assignHandler = assignHandler;
        _closeHandler = closeHandler;
        _reopenHandler = reopenHandler;
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

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        try
        {
            var command = new ResolveTicketCommand
            {
                TicketId = id
            };
            var ticket = await _resolveHandler.Handle(command);
            var response = TicketMapper.ToResponse(ticket);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid id,
        AssignTicketRequest request)
    {
        try
        {
            var command = new AssignTicketCommand
            {
                TicketId = id,
                TechnicianId = request.TechnicianId
            };
            var ticket = await _assignHandler.Handle(command);
            var response = TicketMapper.ToResponse(ticket);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        try
        {
            var command = new CloseTicketCommand
            {
                TicketId = id
            };
            var ticket = await _closeHandler.Handle(command);
            var response = TicketMapper.ToResponse(ticket);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id)
    {
        try
        {
            var command = new ReopenTicketCommand
            {
                TicketId = id
            };
            var ticket = await _reopenHandler.Handle(command);
            var response = TicketMapper.ToResponse(ticket);

            return Ok(response);
        }

        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

}