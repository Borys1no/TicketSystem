using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;
namespace SupportFlow.Application.Commands.Tickets;

public class CreateTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }
    
    public async Task Handle(CreateTicketCommand command)
    {
        var ticket = new Ticket(
            command.Title,
            command.Description,
            command.Priority,
            command.CreatedByUserId
        );
        await _ticketRepository.AddAsync(ticket);
    
    }
}

