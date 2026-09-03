using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;



namespace SupportFlow.Application.Commands.Tickets;

public class ResolveTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;

    public ResolveTicketCommandHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> Handle(ResolveTicketCommand command)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId);
        
        if(ticket is null)
            throw new KeyNotFoundException("Ticket not found.");
        if (ticket.AssignedToUserId != command.TechnicianId)
        {
            throw new UnauthorizedAccessException(
                "You can only resolve tickets assigned to you.");
        }
        ticket.Resolve();

        await _ticketRepository.UpdateAsync(ticket);
        return ticket;
    }
}